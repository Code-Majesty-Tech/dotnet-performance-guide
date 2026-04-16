using Testcontainers.Redis;

namespace CodeMajestyTech.Performance.Shared.TestContainers;

/// <summary>
/// Boots a Redis container for a benchmark class. Use from
/// <c>[GlobalSetup]</c> and dispose in <c>[GlobalCleanup]</c>.
///
/// Configured with LRU eviction and no persistence — appropriate for
/// cache-layer benchmarks where persistence would distort I/O numbers.
/// If you need AOF or RDB persistence, supply a <see cref="StartAsync"/>
/// configure callback.
/// </summary>
public sealed class RedisFixture : IAsyncDisposable
{
    private readonly RedisContainer _container;

    private RedisFixture(RedisContainer container)
    {
        _container = container;
    }

    /// <summary>
    /// StackExchange.Redis-compatible connection string
    /// ("hostname:port"). Safe to pass to
    /// <c>ConnectionMultiplexer.ConnectAsync</c>.
    /// </summary>
    public string ConnectionString => _container.GetConnectionString();

    public ushort MappedPort => _container.GetMappedPublicPort(RedisBuilder.RedisPort);

    public string Hostname => _container.Hostname;

    /// <summary>
    /// Starts a new Redis container and returns once it is ready to accept connections.
    /// </summary>
    /// <param name="configure">
    /// Optional callback for benchmark-specific container configuration
    /// (e.g. enabling AOF, adjusting maxmemory).
    /// </param>
    public static async Task<RedisFixture> StartAsync(
        Action<RedisBuilder>? configure = null,
        CancellationToken cancellationToken = default)
    {
        var builder = new RedisBuilder()
            .WithImage(ContainerImages.Redis)
            .WithCommand(
                "redis-server",
                "--appendonly", "no",
                "--save", "",
                "--maxmemory", "512mb",
                "--maxmemory-policy", "allkeys-lru");

        configure?.Invoke(builder);

        var container = builder.Build();
        await container.StartAsync(cancellationToken).ConfigureAwait(false);
        return new RedisFixture(container);
    }

    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync().ConfigureAwait(false);
    }
}