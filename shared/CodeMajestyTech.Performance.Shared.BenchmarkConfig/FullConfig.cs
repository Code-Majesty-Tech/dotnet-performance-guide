using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using Perfolizer.Horology;
using Perfolizer.Metrology;

namespace CodeMajestyTech.Performance.Shared.BenchmarkConfig;

/// <summary>
/// Maximum-rigor configuration for the most important benchmarks —
/// for example, headline numbers used in the hero chart of a blog post.
/// Uses <see cref="Job.LongRun"/> which performs substantially more
/// warmup and iterations. Expect single benchmark classes to take
/// 10+ minutes. Do not use for routine development.
/// </summary>
public sealed class FullConfig : ManualConfig
{
    public FullConfig()
    {
        AddJob(Job.LongRun
            .WithId("Full"));

        AddDiagnoser(MemoryDiagnoser.Default);
        AddDiagnoser(ThreadingDiagnoser.Default);

        AddExporter(MarkdownExporter.GitHub);
        AddExporter(JsonExporter.Full);
        AddExporter(CsvExporter.Default);

        AddLogger(ConsoleLogger.Default);

        AddColumnProvider(DefaultColumnProviders.Instance);
        AddColumn(BaselineRatioColumn.RatioMean);

        SummaryStyle = BenchmarkDotNet.Reports.SummaryStyle.Default
            .WithTimeUnit(TimeUnit.Microsecond)
            .WithSizeUnit(SizeUnit.KB);
    }
}