namespace CodeMajestyTech.Performance.Shared.TestContainers;

/// <summary>
/// Pinned container image versions used across every benchmark.
/// Bump versions deliberately — benchmark numbers are only meaningful
/// against known container versions. When you bump any image here,
/// re-run affected benchmarks and record new numbers in /results.
/// </summary>
public static class ContainerImages
{
    public const string Postgres = "postgres:16.4-alpine";
    public const string Redis = "redis:7.4-alpine";
    public const string RabbitMq = "rabbitmq:3.13-management-alpine";
    public const string PgBouncer = "edoburu/pgbouncer:1.23.1";
}