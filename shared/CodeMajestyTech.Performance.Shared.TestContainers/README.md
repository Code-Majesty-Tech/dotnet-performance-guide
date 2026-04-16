# Code Majesty Tech — .NET Performance Guide

The practical .NET 10 performance optimization guide. Every post in this series ships with a runnable BenchmarkDotNet project you can clone and run with a single command — supporting services (Postgres, Redis, RabbitMQ) are spun up automatically via Testcontainers.

Built and maintained by [Code Majesty Tech](https://codemajesty.tech) — a boutique .NET development agency.

---

## What's in here

Ten benchmark projects covering the full stack Code Majesty Tech ships in production — ASP.NET Core APIs, EF Core on PostgreSQL, Redis caching, RabbitMQ messaging, and Blazor. Each subfolder maps to a blog post on [codemajesty.tech/blog](https://codemajesty.tech/blog).

No theoretical advice. Every recommendation is backed by BenchmarkDotNet output in the [`results/`](./results) folder.

---

## The series

| # | Post | Folder | Stack |
|---|------|--------|-------|
| 1 | FastEndpoints vs Minimal API vs Controllers | [`01-fastendpoints-vs-minimal-vs-controllers`](./benchmarks/01-fastendpoints-vs-minimal-vs-controllers) | ASP.NET Core |
| 2 | The N+1 Query Problem in EF Core | [`02-ef-core-n-plus-one`](./benchmarks/02-ef-core-n-plus-one) | EF Core / PostgreSQL |
| 3 | HybridCache in .NET 10 | [`03-hybridcache-benchmarks`](./benchmarks/03-hybridcache-benchmarks) | Caching / Redis |
| 4 | System.Text.Json vs Newtonsoft.Json | [`04-json-serialization`](./benchmarks/04-json-serialization) | .NET Runtime |
| 5 | EF Core vs Dapper vs Raw SQL on PostgreSQL | [`05-ef-core-vs-dapper-postgresql`](./benchmarks/05-ef-core-vs-dapper-postgresql) | EF Core / PostgreSQL |
| 6 | Blazor Server vs WebAssembly Performance | [`06-blazor-server-vs-wasm`](./benchmarks/06-blazor-server-vs-wasm) | Blazor |
| 7 | MassTransit + RabbitMQ Performance Tuning | [`07-masstransit-rabbitmq-performance`](./benchmarks/07-masstransit-rabbitmq-performance) | Messaging |
| 8 | Native AOT vs JIT for ASP.NET Core APIs | [`08-native-aot-vs-jit`](./benchmarks/08-native-aot-vs-jit) | .NET Runtime / API |
| 9 | Output Caching in ASP.NET Core with Redis | [`09-output-caching-benchmarks`](./benchmarks/09-output-caching-benchmarks) | API / Caching |
| 10 | PostgreSQL Connection Pooling: Npgsql vs PgBouncer | [`10-postgresql-connection-pooling`](./benchmarks/10-postgresql-connection-pooling) | PostgreSQL |

Links to published blog posts will be added to each folder's README as they go live.

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- A running Docker daemon — [Docker Desktop](https://www.docker.com/products/docker-desktop), [Rancher Desktop](https://rancherdesktop.io), [Colima](https://github.com/abiosoft/colima), or any [Testcontainers-supported runtime](https://java.testcontainers.org/supported_docker_environment/). Testcontainers talks to the daemon directly — no `docker compose` knowledge required.
- At least 8 GB RAM (BenchmarkDotNet forks processes per benchmark and containers boot in parallel)

The `global.json` pins the SDK version. If `dotnet --version` reports a mismatch after cloning, install the matching SDK.

---

## Running a benchmark

Every benchmark follows the same two-step pattern:

```bash
# 1. Move into the benchmark folder
cd benchmarks/01-fastendpoints-vs-minimal-vs-controllers

# 2. Run the benchmark in Release mode
dotnet run -c Release --project ./Benchmarks
```

That's it. Testcontainers boots the services the benchmark needs (Postgres, Redis, RabbitMQ, PgBouncer, etc.), seeds them, runs the benchmark, and tears everything down on exit. No manual container management.

Results are written to `BenchmarkDotNet.Artifacts/` inside each benchmark folder and are not committed. The curated results used in blog posts live under [`results/`](./results).

---

## Why Testcontainers

Every benchmark spins up its own containers in isolation:

- **One command to reproduce** — no "oh I forgot to start the services" moments
- **No port conflicts** — Testcontainers uses random high ports, so you can run multiple benchmarks in parallel
- **Guaranteed clean state** — every run starts from a known database/cache state
- **CI-friendly** — benchmarks run in GitHub Actions without additional setup

Container image versions are pinned in C# constants inside [`shared/CodeMajestyTech.Performance.Shared.TestContainers`](./shared/CodeMajestyTech.Performance.Shared.TestContainers) so numbers stay reproducible over time.

---

## Repository structure

```
codemajestytech-dotnet-performance-guide/
├── CodeMajestyTech.Performance.sln
├── Directory.Build.props              # Shared MSBuild settings (TFM, nullable, analyzers)
├── Directory.Packages.props           # Central Package Management — all package versions
├── global.json                        # Pinned .NET SDK
├── shared/
│   ├── CodeMajestyTech.Performance.Shared.BenchmarkConfig/
│   └── CodeMajestyTech.Performance.Shared.TestContainers/
├── benchmarks/
│   ├── 01-fastendpoints-vs-minimal-vs-controllers/
│   ├── 02-ef-core-n-plus-one/
│   └── ... (one folder per post)
├── results/                           # Curated BenchmarkDotNet output referenced in posts
└── docs/
    ├── METHODOLOGY.md
    └── HARDWARE.md
```

Each benchmark subfolder is self-contained — its own projects, its own README pointing to the blog post, no shared infrastructure at runtime other than the Testcontainers fixtures.

---

## Methodology

Numbers from this repository are only comparable when produced under the same conditions. Each blog post records:

- The hardware the numbers were produced on (CPU, RAM, storage, OS)
- The .NET SDK version and any non-default runtime flags
- Container image versions (pinned in `shared/CodeMajestyTech.Performance.Shared.TestContainers`)
- BenchmarkDotNet job configuration (warmup, iteration count)

See [`docs/METHODOLOGY.md`](./docs/METHODOLOGY.md) for the full methodology and [`docs/HARDWARE.md`](./docs/HARDWARE.md) for reference hardware.

**Important:** These benchmarks are designed for relative comparisons between approaches on a single machine. Absolute numbers on your hardware will differ. Rerun locally before making production decisions.

---

## Contributing

Found a better benchmark configuration? A missing scenario? Open an issue or PR. The goal of this repository is accurate, reproducible numbers — not defending any particular framework or approach.

When submitting a benchmark PR:

1. Include the raw BenchmarkDotNet output in the PR description
2. State the hardware the numbers were produced on
3. Keep changes scoped to one benchmark folder per PR

---

## Work with Code Majesty Tech

This repository is open source and free to use. If your team needs senior .NET engineers who treat performance as a first-class concern:

- **1-Hour Consulting** — $80/hr. Architecture review, performance diagnostics, code review. [Book a session →](https://codemajesty.tech)
- **Sprint-Based Development** — From $60/hr. Dedicated senior .NET developers in 2-week sprints. No long-term contracts. [Learn more →](https://codemajesty.tech)
- **End-to-End Development** — From $20K. Full .NET builds on our production-ready SaaS template. [See the template →](https://codemajesty.tech)

---

## License

[MIT](./LICENSE) — use it, fork it, ship it.

Copyright © Code Majesty Tech.