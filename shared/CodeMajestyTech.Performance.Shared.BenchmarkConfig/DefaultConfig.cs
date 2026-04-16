using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using Perfolizer.Horology;
using Perfolizer.Metrology;

namespace CodeMajestyTech.Performance.Shared.BenchmarkConfig;

/// <summary>
///     Standard configuration used for benchmark numbers published on the
///     Code Majesty Tech blog. Balances rigor with reasonable run time —
///     full runs typically complete in a few minutes per benchmark class.
///     Key differences from <see cref="QuickConfig" />:
///     - Default job (full warmup, ~15 iterations by BenchmarkDotNet defaults)
///     - Baseline ratio column for easy "X times faster" framing in posts
/// </summary>
public sealed class DefaultConfig : ManualConfig
{
    public DefaultConfig()
    {
        AddJob(Job.Default
            .WithWarmupCount(10)
            .WithIterationCount(15)
            .WithId("Standard"));

        AddDiagnoser(MemoryDiagnoser.Default);

        AddExporter(MarkdownExporter.GitHub);
        AddExporter(JsonExporter.Full);
        AddExporter(CsvExporter.Default);

        AddLogger(ConsoleLogger.Default);

        AddColumnProvider(DefaultColumnProviders.Instance);
        AddColumn(BaselineRatioColumn.RatioMean);

        SummaryStyle = SummaryStyle.Default
            .WithTimeUnit(TimeUnit.Microsecond)
            .WithSizeUnit(SizeUnit.KB);
    }
}