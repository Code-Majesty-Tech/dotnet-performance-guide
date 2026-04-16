```

BenchmarkDotNet v0.15.4, Linux Ubuntu 24.04.3 LTS (Noble Numbat)
AMD EPYC-Genoa Processor 2.40GHz, 1 CPU, 8 logical and 8 physical cores
.NET SDK 10.0.104
  [Host] : .NET 10.0.4 (10.0.4, 10.0.426.12010), X64 RyuJIT x86-64-v4
  Full   : .NET 10.0.4 (10.0.4, 10.0.426.12010), X64 RyuJIT x86-64-v4

Job=Full  IterationCount=100  LaunchCount=3  
WarmupCount=15  

```
| Method                    | Mean         | Error        | StdDev      | Median       | Ratio | RatioSD | Completed Work Items | Lock Contentions | Gen0    | Gen1    | Allocated   | Alloc Ratio |
|-------------------------- |-------------:|-------------:|------------:|-------------:|------:|--------:|---------------------:|-----------------:|--------:|--------:|------------:|------------:|
| &#39;Lazy loading (N+1)&#39;      | 336,877.1 μs | 18,284.94 μs | 95,134.3 μs | 345,129.0 μs | 1.089 |    0.45 |             959.0000 |                - |       - |       - | 10843.28 KB |        1.00 |
| &#39;Eager loading (Include)&#39; |   5,370.0 μs |    255.31 μs |  1,330.6 μs |   5,592.1 μs | 0.017 |    0.01 |              12.5938 |                - | 46.8750 | 15.6250 |    889.3 KB |        0.08 |
| &#39;Split query&#39;             |   6,983.5 μs |    389.40 μs |  1,977.0 μs |   7,548.2 μs | 0.023 |    0.01 |               8.4844 |                - | 70.3125 | 23.4375 |  1232.22 KB |        0.11 |
| &#39;Projection (Select)&#39;     |   2,877.1 μs |     97.44 μs |    491.1 μs |   2,745.5 μs | 0.009 |    0.00 |              10.5586 |                - | 19.5313 |  3.9063 |   343.74 KB |        0.03 |
| &#39;Compiled projection&#39;     |   2,425.7 μs |     34.93 μs |    178.3 μs |   2,406.9 μs | 0.008 |    0.00 |               7.2422 |           0.0313 | 19.5313 |       - |   327.11 KB |        0.03 |
| &#39;Raw SQL (manual JOIN)&#39;   |   2,425.4 μs |     98.67 μs |    495.6 μs |   2,274.1 μs | 0.008 |    0.00 |               7.5938 |           0.0039 | 15.6250 |  3.9063 |   312.14 KB |        0.03 |