```

BenchmarkDotNet v0.15.4, Linux Ubuntu 24.04.3 LTS (Noble Numbat)
AMD EPYC-Genoa Processor 2.40GHz, 1 CPU, 8 logical and 8 physical cores
.NET SDK 10.0.104
  [Host] : .NET 10.0.4 (10.0.4, 10.0.426.12010), X64 RyuJIT x86-64-v4
  Full   : .NET 10.0.4 (10.0.4, 10.0.426.12010), X64 RyuJIT x86-64-v4

Job=Full  InvocationCount=1  IterationCount=100  
LaunchCount=3  UnrollFactor=1  WarmupCount=15  

```

| Method              | Framework         |           Mean |         Error |         StdDev | Completed Work Items | Lock Contentions |     Allocated |
|---------------------|-------------------|---------------:|--------------:|---------------:|---------------------:|-----------------:|--------------:|
| **GetSingleEntity** | **Controllers**   | **4,259.4 μs** |  **98.34 μs** |   **504.6 μs** |           **4.0000** |            **-** | **278.71 KB** |
| **GetSingleEntity** | **FastEndpoints** | **3,546.2 μs** | **117.26 μs** |   **600.7 μs** |           **4.0000** |            **-** |  **98.46 KB** |
| **GetSingleEntity** | **MinimalApi**    | **3,697.8 μs** | **113.38 μs** |   **585.9 μs** |           **4.0000** |            **-** |  **97.79 KB** |
|                     |                   |                |               |                |                      |                  |               |
| **GetPagedList**    | **Controllers**   | **5,963.1 μs** | **144.13 μs** |   **747.3 μs** |           **6.0000** |            **-** | **316.85 KB** |
| **GetPagedList**    | **FastEndpoints** | **5,130.7 μs** | **178.18 μs** |   **915.9 μs** |           **6.0000** |            **-** | **133.83 KB** |
| **GetPagedList**    | **MinimalApi**    | **5,232.0 μs** | **167.97 μs** |   **867.9 μs** |           **6.0000** |            **-** | **132.23 KB** |
|                     |                   |                |               |                |                      |                  |               |
| **CreateEntity**    | **Controllers**   | **6,988.4 μs** | **167.65 μs** |   **869.3 μs** |           **6.0000** |            **-** | **335.09 KB** |
| **CreateEntity**    | **FastEndpoints** | **6,075.2 μs** | **204.32 μs** | **1,055.8 μs** |           **6.0000** |            **-** |  **134.6 KB** |
| **CreateEntity**    | **MinimalApi**    | **6,225.0 μs** | **188.61 μs** |   **976.3 μs** |           **6.0000** |            **-** | **144.87 KB** |
|                     |                   |                |               |                |                      |                  |               |
| **UpdateEntity**    | **Controllers**   | **7,038.0 μs** | **274.38 μs** | **1,410.4 μs** |           **9.0000** |            **-** | **175.23 KB** |
| **UpdateEntity**    | **FastEndpoints** | **7,458.1 μs** | **273.36 μs** | **1,424.7 μs** |           **8.0000** |            **-** | **142.09 KB** |
| **UpdateEntity**    | **MinimalApi**    | **7,094.8 μs** | **221.99 μs** | **1,151.1 μs** |           **8.0000** |            **-** | **147.18 KB** |