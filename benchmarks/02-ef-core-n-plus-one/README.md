# Post 02: The N+1 Query Problem in EF Core — Benchmarking Every Fix on PostgreSQL

## What This Measures

Quantifies the cost of the **N+1 query problem** in EF Core 10 against PostgreSQL 16, then benchmarks five canonical
fixes against the same workload. Every benchmark loads 100 orders with their items and product details, doing identical
downstream work — so the only variable is how the data is fetched.

### Benchmark Scenarios

| Benchmark                  | Approach                                                          | Expected SQL roundtrips |
|----------------------------|-------------------------------------------------------------------|-------------------------|
| `LazyLoading_NPlus1`       | Lazy-loading proxies; nav property access triggers per-row SELECT | ~600                    |
| `EagerLoading_Include`     | `.Include(o => o.Items).ThenInclude(i => i.Product)`              | 1                       |
| `SplitQuery_AsSplitQuery`  | Same `Include` chain plus `.AsSplitQuery()`                       | 3                       |
| `Projection_Select`        | `.Select(...)` projection straight to a DTO graph                 | 1                       |
| `CompiledQuery_Projection` | `EF.CompileAsyncQuery` + the same projection                      | 1                       |
| `RawSql_FromSql`           | `Database.SqlQueryRaw` with a hand-written `LEFT JOIN`            | 1                       |

`LazyLoading_NPlus1` is the BenchmarkDotNet baseline — every other row in the report is annotated with a "ratio" column
that reads as "how many times faster than the N+1 path."

### Infrastructure

- **PostgreSQL 16** via Testcontainers (auto-started, no Docker Compose needed).
- **Seed data**: 50 categories, 1,000 products, 10,000 orders, ~5 items per order (deterministic seed so query counts
  are reproducible).
- **Connection pooling** uses Npgsql defaults to keep the comparison about query shape, not pool tuning.

## How to Run

```bash
cd benchmarks/02-ef-core-n-plus-one
dotnet run -c Release --project benchmarks/CodeMajestyTech.Performance.Post02.Benchmarks
```

**Requirements**: Docker must be running (for the Testcontainers PostgreSQL container).

**Expected runtime**: ~6–10 minutes with the default `DefaultConfig` profile (10 warmup, 15 measurement iterations × 6
scenarios). The N+1 baseline alone takes the longest (hundreds of round-trips per iteration).

### Surfacing the SQL query count

Time and allocations come straight from BenchmarkDotNet, but the *number of SQL queries* — the headline number for the
post — is captured separately. Run the calibration mode to print it:

```bash
dotnet run -c Release --project benchmarks/CodeMajestyTech.Performance.Post02.Benchmarks -- --calibrate
```

This boots the same container, runs each scenario once with a `DbCommandInterceptor` attached, and prints a query-count
table. The interceptor is intentionally **not** registered during the BenchmarkDotNet run so it does not pollute the
timing numbers.

Raw results are written to `BenchmarkDotNet.Artifacts/` (gitignored). Curated results are committed at:

- For MacOS[
  `results/02-ef-core-n-plus-one/CodeMajestyTech.Performance.Post02.Benchmarks.NplusOneBenchmarks-report-github-macos.md`](../../results/02-ef-core-n-plus-one/CodeMajestyTech.Performance.Post02.Benchmarks.NplusOneBenchmarks-report-github-macos.md).
- For Ubuntu[
  `results/02-ef-core-n-plus-one/CodeMajestyTech.Performance.Post02.Benchmarks.NplusOneBenchmarks-report-github-ubuntu.md`](../../results/02-ef-core-n-plus-one/CodeMajestyTech.Performance.Post02.Benchmarks.NplusOneBenchmarks-report-github-ubuntu.md).


## Project Structure

```
src/
└── CodeMajestyTech.Performance.Post02.DataAccess/   # Models (Order/OrderItem/Product/Category),
                                                     # OrdersDbContext, DTOs, DataSeeder,
                                                     # QueryCounterInterceptor
benchmarks/
└── CodeMajestyTech.Performance.Post02.Benchmarks/   # NplusOneBenchmarks + calibration entry point
```

## Blog Post

[The N+1 Query Problem in EF Core: Benchmarking Every Fix on PostgreSQL](https://codemajesty.tech/blog/ef-core-n-plus-one-problem-postgresql-benchmarks/)
