using System.Net.Http.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using CodeMajestyTech.Performance.Post01.Shared;
using CodeMajestyTech.Performance.Shared.BenchmarkConfig;
using CodeMajestyTech.Performance.Shared.TestContainers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using FastEndpointsMarker = CodeMajestyTech.Performance.Post01.Api.FastEndpoints.AssemblyMarker;
using MinimalApiMarker = CodeMajestyTech.Performance.Post01.Api.MinimalApi.AssemblyMarker;
using ControllersMarker = CodeMajestyTech.Performance.Post01.Api.Controllers.AssemblyMarker;

namespace CodeMajestyTech.Performance.Post01.Benchmarks;

[Config(typeof(FullConfig))]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByMethod)]
public class ApiBenchmarks
{
    private Dictionary<string, HttpClient> _clients = null!;
    private WebApplicationFactory<FastEndpointsMarker> _feFactory = null!;
    private WebApplicationFactory<MinimalApiMarker> _minFactory = null!;
    private WebApplicationFactory<ControllersMarker> _mvcFactory = null!;
    private PostgresFixture _postgres = null!;

    [Params("FastEndpoints", "MinimalApi", "Controllers")]
    public string Framework { get; set; } = null!;

    private HttpClient Client => _clients[Framework];

    [GlobalSetup]
    public async Task Setup()
    {
        _postgres = await PostgresFixture.StartAsync();
        await DataSeeder.SeedAsync(_postgres.ConnectionString);

        _feFactory = CreateFactory<FastEndpointsMarker>();
        _minFactory = CreateFactory<MinimalApiMarker>();
        _mvcFactory = CreateFactory<ControllersMarker>();

        _clients = new Dictionary<string, HttpClient>
        {
            ["FastEndpoints"] = _feFactory.CreateClient(),
            ["MinimalApi"] = _minFactory.CreateClient(),
            ["Controllers"] = _mvcFactory.CreateClient()
        };

        // Warm up each API with a single request
        foreach (var client in _clients.Values)
            await client.GetAsync("/products/1");
    }

    private WebApplicationFactory<T> CreateFactory<T>() where T : class
    {
        return new WebApplicationFactory<T>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:Benchmark", _postgres.ConnectionString);
            });
    }

    [IterationCleanup]
    public void CleanupCreatedProducts()
    {
        using var db = new BenchmarkDbContext(
            new DbContextOptionsBuilder<BenchmarkDbContext>()
                .UseNpgsql(_postgres.ConnectionString)
                .Options);

        db.Products.Where(p => p.Id > 1000).ExecuteDelete();
    }

    [GlobalCleanup]
    public async Task Cleanup()
    {
        foreach (var client in _clients.Values)
            client.Dispose();

        _feFactory.Dispose();
        _minFactory.Dispose();
        _mvcFactory.Dispose();
        await _postgres.DisposeAsync();
    }

    [Benchmark]
    public async Task<string> GetSingleEntity()
    {
        var response = await Client.GetAsync("/products/1");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [Benchmark]
    public async Task<string> GetPagedList()
    {
        var response = await Client.GetAsync("/products?page=1&size=20");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [Benchmark]
    public async Task<string> CreateEntity()
    {
        var request = new CreateProductRequest(
            $"Benchmark Product {Guid.NewGuid():N}",
            $"BM-{Guid.NewGuid():N}",
            "Benchmark test product",
            29.99m,
            100,
            1);
        var response = await Client.PostAsJsonAsync("/products", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [Benchmark]
    public async Task<string> UpdateEntity()
    {
        var request = new UpdateProductRequest(
            $"Updated Product {Guid.NewGuid():N}",
            "Updated description",
            39.99m,
            50);
        var response = await Client.PutAsJsonAsync("/products/1", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}