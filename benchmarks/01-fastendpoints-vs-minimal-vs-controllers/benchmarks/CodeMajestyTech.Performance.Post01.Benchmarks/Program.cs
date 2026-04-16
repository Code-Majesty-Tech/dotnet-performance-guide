using BenchmarkDotNet.Running;
using CodeMajestyTech.Performance.Post01.Benchmarks;

BenchmarkRunner.Run<ApiBenchmarks>(args: args);