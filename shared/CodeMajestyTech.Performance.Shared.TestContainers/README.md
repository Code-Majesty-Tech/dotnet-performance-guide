# CodeMajestyTech.Performance.Shared.TestContainers

Shared Testcontainers fixtures used across every benchmark project. Each fixture spins up one container, exposes a connection string, and cleans up on disposal. Designed to plug directly into BenchmarkDotNet's `[GlobalSetup]` / `[GlobalCleanup]` lifecycle.

## Available fixtures

| Fixture | Image | Used by |
|---------|-------|---------|
| `PostgresFixture` | `postgres:16.4-alpine` | Posts 2, 5, 9, 10 |
| `RedisFixture` | `redis:7.4-alpine` | Posts 3, 9 |
| `RabbitMqFixture` | `rabbitmq:3.13-management-alpine` | Post 7 |
| `PgBouncerFixture` | `edoburu/pgbouncer:1.23.1` | Post 10 |

All image versions are pinned in [`ContainerImages.cs`](./ContainerImages.cs). Change them deliberately and re-run affected benchmarks.

## Standard usage pattern

Reference the project from your benchmark csproj:

```xml
<ProjectReference Include="..\..\..\shared\CodeMajestyTech.Performance.Shared.TestContainers\CodeMajestyTech.Performance.Shared.TestContainers.csproj" />
```

Then use a fixture from any benchmark class:

```csharp
using BenchmarkDotNet.Attributes;
using CodeMajestyTech.Performance.Shared.BenchmarkConfig;
using CodeMajestyTech.Performance.Shared.TestContainers;

[Config(typeof(DefaultConfig))]
public class EfCoreBenchmarks
{
    private PostgresFixture _postgres = null!;

    [GlobalSetup]
    public async Task Setup()
    {
        _postgres = await PostgresFixture.StartAsync();
        // Seed data, run migrations, warm up pool, etc.
    }

    [GlobalCleanup]
    public async Task Cleanup()
    {
        await _postgres.DisposeAsync();
    }

    [Benchmark]
    public async Task QueryProducts()
    {
        // Use _postgres.ConnectionString
    }
}
```

## Customizing container configuration per benchmark

Each `StartAsync` accepts an optional configure callback. This lets individual benchmarks tune container settings without forking the fixture:

```csharp
// Benchmark needs max_connections=50 to force pool exhaustion
_postgres = await PostgresFixture.StartAsync(builder => builder
    .WithCommand("-c", "max_connections=50"));
```

## PgBouncer setup

`PgBouncerFixture` is a special case — PgBouncer needs to talk to Postgres over Docker networking (not localhost), so the two containers must share a network. The simplest pattern:

```csharp
// Starts Postgres + PgBouncer on a shared network in one call.
// Disposing the fixture disposes both containers and the network.
_pgBouncer = await PgBouncerFixture.StartWithPostgresAsync(
    poolMode: PoolMode.Transaction,
    defaultPoolSize: 25);
```

For benchmarks that need direct access to both Postgres and PgBouncer simultaneously (e.g. comparing pooled vs direct connections), use the lower-level `PgBouncerFixture.StartAsync` overload that takes an existing network — see [`PgBouncerFixture.cs`](./PgBouncerFixture.cs) for the API.

## Why `IAsyncDisposable` and not `[ContainerTest]`-style attributes?

BenchmarkDotNet's lifecycle (`[GlobalSetup]`, `[GlobalCleanup]`, `[Params]`) doesn't compose cleanly with xUnit-style fixture attributes. The `IAsyncDisposable` pattern maps one-to-one with BenchmarkDotNet's lifecycle attributes and keeps the dependency surface small — you only need Testcontainers, not xUnit.

## Performance notes

- **Container startup is the dominant cost** of a cold benchmark run. `PostgresFixture` typically takes 3-5 seconds to become ready on a modern machine; Redis is under 1 second; RabbitMQ is the slowest at 10-15 seconds due to Erlang cluster initialization.
- **Run one fixture per benchmark class**, not per benchmark method. BenchmarkDotNet's `[GlobalSetup]` runs once per class.
- **Reuse containers across `[Params]` values** — changing a parameter should not trigger a container restart. Keep config differences at the data-layer level (different seed data, different connection strings) rather than different containers.