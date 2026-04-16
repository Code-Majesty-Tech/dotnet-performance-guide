using Testcontainers.RabbitMq;

namespace CodeMajestyTech.Performance.Shared.TestContainers;

/// <summary>
/// Boots a RabbitMQ container (with management plugin) for a benchmark
/// class. Use from <c>[GlobalSetup]</c> and dispose in
/// <c>[GlobalCleanup]</c>.
///
/// The management plugin is included because MassTransit's health checks
/// and a number of tuning benchmarks read queue statistics from the
/// management API.
/// </summary>
public sealed class RabbitMqFixture : IAsyncDisposable
{
    private readonly RabbitMqContainer _container;

    private RabbitMqFixture(RabbitMqContainer container)
    {
        _container = container;
    }

    /// <summary>
    /// AMQP URI for clients such as MassTransit or RabbitMQ.Client.
    /// </summary>
    public string ConnectionString => _container.GetConnectionString();

    public ushort AmqpPort => _container.GetMappedPublicPort(RabbitMqBuilder.RabbitMqPort);

    public string Hostname => _container.Hostname;

    public const string Username = "benchmark";
    public const string Password = "benchmark";

    /// <summary>
    /// Starts a new RabbitMQ container and returns once the broker is ready.
    /// </summary>
    /// <param name="configure">
    /// Optional callback for benchmark-specific container configuration
    /// (e.g. enabling additional plugins, adjusting memory high watermark).
    /// </param>
    public static async Task<RabbitMqFixture> StartAsync(
        Action<RabbitMqBuilder>? configure = null,
        CancellationToken cancellationToken = default)
    {
        var builder = new RabbitMqBuilder()
            .WithImage(ContainerImages.RabbitMq)
            .WithUsername(Username)
            .WithPassword(Password);

        configure?.Invoke(builder);

        var container = builder.Build();
        await container.StartAsync(cancellationToken).ConfigureAwait(false);
        return new RabbitMqFixture(container);
    }

    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync().ConfigureAwait(false);
    }
}