# Post 01: FastEndpoints vs Minimal API vs Controllers — .NET 10 Performance Benchmarks

## What This Measures

Compares the request pipeline overhead of three ASP.NET Core API frameworks — **FastEndpoints**, **Minimal APIs**, and *
*MVC Controllers** — performing identical CRUD operations against PostgreSQL with EF Core.

All three APIs share the same `ProductService` and `BenchmarkDbContext`, isolating the framework pipeline as the only
variable: routing, model binding, validation (FluentValidation), and JSON serialization.

### Benchmark Scenarios

| Benchmark         | HTTP Call                      | What It Tests                                                |
|-------------------|--------------------------------|--------------------------------------------------------------|
| `GetSingleEntity` | `GET /products/1`              | Single-row fetch + serialization                             |
| `GetPagedList`    | `GET /products?page=1&size=20` | Paginated query + collection serialization                   |
| `CreateEntity`    | `POST /products`               | Deserialization + validation + insert                        |
| `UpdateEntity`    | `PUT /products/1`              | Deserialization + validation + optimistic concurrency update |

### Infrastructure

- **PostgreSQL 16** via Testcontainers (auto-started, no Docker Compose needed)
- **Seed data**: 50 categories, 1,000 products
- **In-process testing** via `WebApplicationFactory` (no network hop)

## How to Run

```bash
cd benchmarks/01-fastendpoints-vs-minimal-vs-controllers
dotnet run -c Release --project benchmarks/CodeMajestyTech.Performance.Post01.Benchmarks
```

**Requirements**: Docker must be running (for Testcontainers PostgreSQL).

**Expected runtime**: ~15–25 minutes with the FullConfig profile (LongRun job: 15 warmup, 100 measurement iterations, 3
launches per benchmark × 3 frameworks × 4 scenarios).

Raw results are written to `BenchmarkDotNet.Artifacts/` (gitignored). Curated results are committed at:

- For MacOS[
  `results/01-fastendpoints/CodeMajestyTech.Performance.Post01.Benchmarks.ApiBenchmarks-report-github-macos.md`](../../results/01-fastendpoints/CodeMajestyTech.Performance.Post01.Benchmarks.ApiBenchmarks-report-github-macos.md).
- For Ubuntu[
  `results/01-fastendpoints/CodeMajestyTech.Performance.Post01.Benchmarks.ApiBenchmarks-report-github-ubuntu.md`](../../results/01-fastendpoints/CodeMajestyTech.Performance.Post01.Benchmarks.ApiBenchmarks-report-github-ubuntu.md).

## Project Structure

```
src/
├── CodeMajestyTech.Performance.Post01.Shared/          # EF Core models, DTOs, DbContext, ProductService
├── CodeMajestyTech.Performance.Post01.Api.FastEndpoints/ # FastEndpoints API (4 endpoints)
├── CodeMajestyTech.Performance.Post01.Api.MinimalApi/    # Minimal API (4 routes)
└── CodeMajestyTech.Performance.Post01.Api.Controllers/   # MVC Controllers API (ProductsController)
benchmarks/
└── CodeMajestyTech.Performance.Post01.Benchmarks/        # BenchmarkDotNet harness
```

## Blog Post

[FastEndpoints vs Minimal API vs Controllers: .NET 10 Performance Benchmarks](https://codemajesty.tech/blog/fastendpoints-vs-minimal-api-vs-controllers-dotnet-10-benchmarks/)
