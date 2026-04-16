using System.Text;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;

namespace CodeMajestyTech.Performance.Shared.TestContainers;

/// <summary>
/// Boots a PgBouncer connection-pooler container in front of an
/// existing <see cref="PostgresFixture"/>. Use from <c>[GlobalSetup]</c>
/// and dispose in <c>[GlobalCleanup]</c>.
///
/// Because PgBouncer needs to talk to Postgres over container networking
/// (not through the host), this fixture creates a shared Docker network
/// that both containers attach to. Callers should start the Postgres
/// fixture first via <see cref="PostgresFixture.StartAsync"/> overload
/// that accepts a configure callback — then pass the same network here.
///
/// For the typical case, prefer the high-level helper
/// <see cref="StartWithPostgresAsync"/> which handles network creation
/// for both containers in one call.
/// </summary>
public sealed class PgBouncerFixture : IAsyncDisposable
{
    private readonly IContainer _container;
    private readonly INetwork? _ownedNetwork;
    private readonly PostgresFixture? _ownedPostgres;

    private PgBouncerFixture(
        IContainer container,
        INetwork? ownedNetwork,
        PostgresFixture? ownedPostgres)
    {
        _container = container;
        _ownedNetwork = ownedNetwork;
        _ownedPostgres = ownedPostgres;
    }

    /// <summary>
    /// Npgsql-compatible connection string pointing at PgBouncer. Use
    /// this in place of the underlying Postgres connection string when
    /// benchmarking the pooler.
    /// </summary>
    public string ConnectionString { get; private set; } = string.Empty;

    public ushort MappedPort => _container.GetMappedPublicPort(PgBouncerPort);

    public const ushort PgBouncerPort = 5432;
    public const string Username = PostgresFixture.Username;
    public const string Password = PostgresFixture.Password;
    public const string Database = PostgresFixture.Database;

    /// <summary>
    /// One-call helper that creates a Docker network, starts Postgres
    /// on it, then starts PgBouncer in front of Postgres. The returned
    /// fixture owns the Postgres container and will dispose it too.
    /// </summary>
    public static async Task<PgBouncerFixture> StartWithPostgresAsync(
        PoolMode poolMode = PoolMode.Transaction,
        int maxClientConnections = 1000,
        int defaultPoolSize = 25,
        CancellationToken cancellationToken = default)
    {
        var network = new NetworkBuilder()
            .WithName($"pgbouncer-net-{Guid.NewGuid():N}")
            .Build();

        await network.CreateAsync(cancellationToken).ConfigureAwait(false);

        const string postgresAlias = "postgres";

        var postgres = await PostgresFixture.StartAsync(
            builder => builder
                .WithNetwork(network)
                .WithNetworkAliases(postgresAlias),
            cancellationToken).ConfigureAwait(false);

        var pgBouncer = await BuildPgBouncerAsync(
            network,
            postgresAlias,
            poolMode,
            maxClientConnections,
            defaultPoolSize,
            ownedNetwork: network,
            ownedPostgres: postgres,
            cancellationToken).ConfigureAwait(false);

        return pgBouncer;
    }

    /// <summary>
    /// Starts PgBouncer in front of an externally-owned Postgres
    /// container attached to <paramref name="network"/>. The caller
    /// remains responsible for disposing the Postgres container and
    /// the network.
    /// </summary>
    public static Task<PgBouncerFixture> StartAsync(
        INetwork network,
        string postgresNetworkAlias,
        PoolMode poolMode = PoolMode.Transaction,
        int maxClientConnections = 1000,
        int defaultPoolSize = 25,
        CancellationToken cancellationToken = default)
        => BuildPgBouncerAsync(
            network,
            postgresNetworkAlias,
            poolMode,
            maxClientConnections,
            defaultPoolSize,
            ownedNetwork: null,
            ownedPostgres: null,
            cancellationToken);

    private static async Task<PgBouncerFixture> BuildPgBouncerAsync(
        INetwork network,
        string postgresNetworkAlias,
        PoolMode poolMode,
        int maxClientConnections,
        int defaultPoolSize,
        INetwork? ownedNetwork,
        PostgresFixture? ownedPostgres,
        CancellationToken cancellationToken)
    {
        var container = new ContainerBuilder()
            .WithImage(ContainerImages.PgBouncer)
            .WithNetwork(network)
            .WithEnvironment("DB_USER", Username)
            .WithEnvironment("DB_PASSWORD", Password)
            .WithEnvironment("DB_HOST", postgresNetworkAlias)
            .WithEnvironment("DB_PORT", "5432")
            .WithEnvironment("DB_NAME", Database)
            .WithEnvironment("POOL_MODE", poolMode.ToPgBouncerString())
            .WithEnvironment("MAX_CLIENT_CONN", maxClientConnections.ToString())
            .WithEnvironment("DEFAULT_POOL_SIZE", defaultPoolSize.ToString())
            .WithEnvironment("ADMIN_USERS", Username)
            .WithEnvironment("AUTH_TYPE", "scram-sha-256")
            .WithPortBinding(PgBouncerPort, assignRandomHostPort: true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(PgBouncerPort))
            .Build();

        await container.StartAsync(cancellationToken).ConfigureAwait(false);

        var fixture = new PgBouncerFixture(container, ownedNetwork, ownedPostgres);

        var mappedPort = container.GetMappedPublicPort(PgBouncerPort);
        var builder = new StringBuilder();
        builder.Append("Host=").Append(container.Hostname)
               .Append(";Port=").Append(mappedPort)
               .Append(";Username=").Append(Username)
               .Append(";Password=").Append(Password)
               .Append(";Database=").Append(Database)
               .Append(";Pooling=false");
        fixture.ConnectionString = builder.ToString();

        return fixture;
    }

    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync().ConfigureAwait(false);
        if (_ownedPostgres is not null)
        {
            await _ownedPostgres.DisposeAsync().ConfigureAwait(false);
        }
        if (_ownedNetwork is not null)
        {
            await _ownedNetwork.DisposeAsync().ConfigureAwait(false);
        }
    }
}

/// <summary>
/// PgBouncer pool modes. Transaction mode is the standard choice for
/// ASP.NET Core APIs running short transactions; Session mode behaves
/// like a traditional connection pool and is required for features
/// that rely on session state (e.g. LISTEN/NOTIFY, prepared statements).
/// </summary>
public enum PoolMode
{
    Transaction,
    Session,
    Statement,
}

internal static class PoolModeExtensions
{
    public static string ToPgBouncerString(this PoolMode mode) => mode switch
    {
        PoolMode.Transaction => "transaction",
        PoolMode.Session => "session",
        PoolMode.Statement => "statement",
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown PgBouncer pool mode."),
    };
}