using BenchmarkDotNet.Attributes;
using CodeMajestyTech.Performance.Post02.DataAccess;
using CodeMajestyTech.Performance.Shared.BenchmarkConfig;
using CodeMajestyTech.Performance.Shared.TestContainers;
using Microsoft.EntityFrameworkCore;

namespace CodeMajestyTech.Performance.Post02.Benchmarks;

[Config(typeof(FullConfig))]
[BenchmarkCategory("Queries")]
public class NplusOneBenchmarks
{
    public const int OrdersToLoad = 100;

    private static readonly Func<OrdersDbContext, int, IAsyncEnumerable<OrderSummaryDto>> CompiledProjectionQuery =
        EF.CompileAsyncQuery((OrdersDbContext db, int take) =>
            db.Orders
                .OrderBy(o => o.Id)
                .Take(take)
                .Select(o => new OrderSummaryDto(
                    o.Id,
                    o.CustomerEmail,
                    o.PlacedAt,
                    o.Total,
                    o.Items.Select(i => new OrderItemDto(
                        i.ProductId,
                        i.Product.Name,
                        i.Product.Sku,
                        i.Quantity,
                        i.UnitPrice)).ToList())));

    private DbContextOptions<OrdersDbContext> _lazyOptions = null!;
    private DbContextOptions<OrdersDbContext> _options = null!;
    private PostgresFixture _postgres = null!;

    [GlobalSetup]
    public async Task Setup()
    {
        _postgres = await PostgresFixture.StartAsync();
        await DataSeeder.SeedAsync(_postgres.ConnectionString);

        _options = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseNpgsql(_postgres.ConnectionString)
            .Options;

        _lazyOptions = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseNpgsql(_postgres.ConnectionString)
            .UseLazyLoadingProxies()
            .Options;
    }

    [GlobalCleanup]
    public async Task Cleanup()
    {
        await _postgres.DisposeAsync();
    }

    private OrdersDbContext NewContext()
    {
        return new OrdersDbContext(_options);
    }

    private OrdersDbContext NewLazyContext()
    {
        return new OrdersDbContext(_lazyOptions);
    }

    [Benchmark(Baseline = true, Description = "Lazy loading (N+1)")]
    public async Task<int> LazyLoading_NPlus1()
    {
        // Lazy-loading proxies require change tracking — AsNoTracking would
        // silently return empty navigation collections.
        await using var db = NewLazyContext();
        var orders = await db.Orders
            .OrderBy(o => o.Id)
            .Take(OrdersToLoad)
            .ToListAsync();

        var sum = 0;
        foreach (var order in orders)
            // Each access to order.Items triggers a SELECT for that order's items.
            // Each access to item.Product triggers a SELECT for that product.
        foreach (var item in order.Items)
            sum += item.Quantity + item.Product.Name.Length;
        return sum;
    }

    [Benchmark(Description = "Eager loading (Include)")]
    public async Task<int> EagerLoading_Include()
    {
        await using var db = NewContext();
        var orders = await db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .OrderBy(o => o.Id)
            .Take(OrdersToLoad)
            .ToListAsync();

        var sum = 0;
        foreach (var order in orders)
        foreach (var item in order.Items)
            sum += item.Quantity + item.Product.Name.Length;
        return sum;
    }

    [Benchmark(Description = "Split query")]
    public async Task<int> SplitQuery_AsSplitQuery()
    {
        await using var db = NewContext();
        var orders = await db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .AsSplitQuery()
            .OrderBy(o => o.Id)
            .Take(OrdersToLoad)
            .ToListAsync();

        var sum = 0;
        foreach (var order in orders)
        foreach (var item in order.Items)
            sum += item.Quantity + item.Product.Name.Length;
        return sum;
    }

    [Benchmark(Description = "Projection (Select)")]
    public async Task<int> Projection_Select()
    {
        await using var db = NewContext();
        var summaries = await db.Orders
            .AsNoTracking()
            .OrderBy(o => o.Id)
            .Take(OrdersToLoad)
            .Select(o => new OrderSummaryDto(
                o.Id,
                o.CustomerEmail,
                o.PlacedAt,
                o.Total,
                o.Items.Select(i => new OrderItemDto(
                    i.ProductId,
                    i.Product.Name,
                    i.Product.Sku,
                    i.Quantity,
                    i.UnitPrice)).ToList()))
            .ToListAsync();

        var sum = 0;
        foreach (var order in summaries)
        foreach (var item in order.Items)
            sum += item.Quantity + item.ProductName.Length;
        return sum;
    }

    [Benchmark(Description = "Compiled projection")]
    public async Task<int> CompiledQuery_Projection()
    {
        await using var db = NewContext();
        var sum = 0;
        await foreach (var order in CompiledProjectionQuery(db, OrdersToLoad))
        foreach (var item in order.Items)
            sum += item.Quantity + item.ProductName.Length;
        return sum;
    }

    [Benchmark(Description = "Raw SQL (manual JOIN)")]
    public async Task<int> RawSql_FromSql()
    {
        await using var db = NewContext();
        var rows = await db.Database.SqlQueryRaw<OrderItemFlatRow>(
                """
                SELECT o."Id" AS "OrderId", o."CustomerEmail", o."PlacedAt", o."Total",
                       i."ProductId", p."Name" AS "ProductName", p."Sku" AS "ProductSku",
                       i."Quantity", i."UnitPrice"
                FROM (SELECT "Id", "CustomerEmail", "PlacedAt", "Total"
                      FROM "Orders" ORDER BY "Id" LIMIT {0}) o
                LEFT JOIN "OrderItems" i ON i."OrderId" = o."Id"
                LEFT JOIN "Products" p ON p."Id" = i."ProductId"
                ORDER BY o."Id"
                """,
                OrdersToLoad)
            .ToListAsync();

        var sum = 0;
        foreach (var row in rows)
            if (row.Quantity is { } qty && row.ProductName is { } name)
                sum += qty + name.Length;
        return sum;
    }
}