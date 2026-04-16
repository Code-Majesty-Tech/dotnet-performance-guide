# CodeMajestyTech.Performance.Shared.BenchmarkConfig

Shared BenchmarkDotNet configuration used across every benchmark project in this repository. Three profiles are provided — pick the one that matches your goal.

## Profiles

| Config | Job | Use case | Approx. time |
|--------|-----|----------|--------------|
| `QuickConfig` | `ShortRun` (3 warmup, 5 iter) | Dev iteration — fast feedback while authoring a benchmark | ~30s per benchmark |
| `DefaultConfig` | `Default` (10 warmup, 15 iter) | Blog-post numbers — the standard for published results | ~2-5 min per class |
| `FullConfig` | `LongRun` | Headline charts, high-stakes comparisons, reproducibility claims | 10+ min per class |

All three profiles include:

- `MemoryDiagnoser` (bytes allocated per operation)
- Markdown / JSON / CSV exporters (Markdown output goes straight into blog posts)
- Statistical test column using Mann-Whitney U

`FullConfig` additionally enables `ThreadingDiagnoser` (lock contention, completed work items) and shows P-values for the statistical tests.

## Usage

Reference the project from your benchmark csproj:

```xml
<ProjectReference Include="..\..\..\shared\CodeMajestyTech.Performance.Shared.BenchmarkConfig\CodeMajestyTech.Performance.Shared.BenchmarkConfig.csproj" />
```

Apply the config via attribute:

```csharp
using BenchmarkDotNet.Attributes;
using CodeMajestyTech.Performance.Shared.BenchmarkConfig;

[Config(typeof(DefaultConfig))]
public class JsonSerializationBenchmarks
{
    [Benchmark(Baseline = true)]
    public string SystemTextJson_Serialize() => /* ... */;

    [Benchmark]
    public string Newtonsoft_Serialize() => /* ... */;
}
```

Or pass it on the command line:

```bash
dotnet run -c Release -- --job short       # maps to QuickConfig-style
dotnet run -c Release -- --job default     # maps to DefaultConfig-style
dotnet run -c Release -- --job long        # maps to FullConfig-style
```

Using the `[Config(typeof(...))]` attribute is preferred because it locks the benchmark class to a specific rigor level and makes blog-post reproducibility explicit.

## Why three profiles?

Benchmark-driven content has a conflict: **fast feedback while authoring** and **statistically rigorous numbers for publication** are opposing goals. Having three named profiles makes the tradeoff explicit rather than fiddling with `WithIterationCount` values each time.

When drafting a benchmark: `QuickConfig`.
When recording numbers for a post: `DefaultConfig`.
When the numbers are the whole point of the post: `FullConfig`.