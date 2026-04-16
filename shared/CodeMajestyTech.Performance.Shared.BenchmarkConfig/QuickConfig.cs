using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;

namespace CodeMajestyTech.Performance.Shared.BenchmarkConfig;

/// <summary>
/// Fast-iteration benchmark configuration for use during development.
/// Single short-run job with minimal warmup/iterations so results appear
/// in under a minute per benchmark. Do NOT use for published numbers —
/// use <see cref="DefaultConfig"/> or <see cref="FullConfig"/> instead.
/// </summary>
public sealed class QuickConfig : ManualConfig
{
    public QuickConfig()
    {
        AddJob(Job.ShortRun
            .WithWarmupCount(3)
            .WithIterationCount(5)
            .WithId("Quick"));

        AddDiagnoser(MemoryDiagnoser.Default);

        AddExporter(MarkdownExporter.GitHub);
        AddExporter(JsonExporter.Full);
        AddExporter(CsvExporter.Default);

        AddLogger(ConsoleLogger.Default);

        AddColumnProvider(DefaultColumnProviders.Instance);

        WithOptions(ConfigOptions.DisableOptimizationsValidator);
    }
}