using Testcontainers.PostgreSql;

namespace CodeMajestyTech.Performance.Shared.TestContainers;

/// <summary>
/// Boots a PostgreSQL container for a benchmark class. Use from
/// <c>[GlobalSetup]</c> and dispose in <c>[GlobalCleanup]</c>.
///
/// Example:
/// <code>
/// private PostgresFixture _postgres = null!;
///
/// [GlobalSetup]
/// public async Task Setup()
/// {
///     _postgres = await PostgresFixture.StartAsync();
///     // use _postgres.ConnectionString ...
/// }
///
/// [GlobalCleanup]
/// public async Task Cleanup() => await _postgres.DisposeAsync();
/// </code>
///
/// The container is configured with performance-oriented defaults
/// (256MB shared_buffers, 200 max_connections) that match what most
/// production .NET deployments run with. Override via
/// <see cref="StartAsync(Action{PostgreSqlBuilder}?)"/> if a benchmark
/// needs specific tuning.
/// </summary>
public sealed class PostgresFixture : IAsyncDisposable
{
    private readonly PostgreSqlContainer _container;

    private PostgresFixture(PostgreSqlContainer container)
    {
        _container = container;
    }

    /// <summary>
    /// Connection string pointing at the running container.
    /// Safe to use immediately after <see cref="StartAsync"/> returns.
    /// </summary>
    public string ConnectionString => _container.GetConnectionString();

    /// <summary>
    /// Host port the container is mapped to. Useful when constructing
    /// connection strings for tools that don't accept a full Npgsql
    /// connection string (e.g. PgBouncer configuration).
    /// </summary>
    public ushort MappedPort => _container.GetMappedPublicPort(PostgreSqlBuilder.PostgreSqlPort);

    /// <summary>
    /// Hostname of the container as seen from the host machine.
    /// Typically "localhost" but may differ under Colima, Podman, etc.
    /// </summary>
    public string Hostname => _container.Hostname;

    public const string Username = "benchmark";
    public const string Password = "benchmark";
    public const string Database = "benchmark";

    /// <summary>
    /// Starts a new Postgres container and returns once it is healthy.
    /// </summary>
    /// <param name="configure">
    /// Optional callback to apply benchmark-specific container config
    /// (e.g. custom postgresql.conf command arguments).
    /// </param>
    public static async Task<PostgresFixture> StartAsync(
        Action<PostgreSqlBuilder>? configure = null,
        CancellationToken cancellationToken = default)
    {
        var builder = new PostgreSqlBuilder()
            .WithImage(ContainerImages.Postgres)
            .WithUsername(Username)
            .WithPassword(Password)
            .WithDatabase(Database)
            .WithCommand(
                "-c", "shared_buffers=256MB",
                "-c", "effective_cache_size=1GB",
                "-c", "maintenance_work_mem=64MB",
                "-c", "max_connections=200",
                "-c", "random_page_cost=1.1");

        configure?.Invoke(builder);

        var container = builder.Build();
        await container.StartAsync(cancellationToken).ConfigureAwait(false);
        return new PostgresFixture(container);
    }

    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync().ConfigureAwait(false);
    }
}