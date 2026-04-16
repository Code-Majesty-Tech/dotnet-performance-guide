using BenchmarkDotNet.Running;
using CodeMajestyTech.Performance.Post02.Benchmarks;
using CodeMajestyTech.Performance.Post02.DataAccess;
using CodeMajestyTech.Performance.Shared.TestContainers;
using Microsoft.EntityFrameworkCore;

// Optional calibration mode: prints the SQL command count for each scenario
// so the post can quote "501 queries vs 1 query" alongside time/alloc numbers.
// Run with `dotnet run -c Release -- --calibrate`. Otherwise BenchmarkDotNet
// takes over.
if (args is ["--calibrate"])
{
    await CalibrateQueryCountsAsync();
    return;
}

BenchmarkRunner.Run<NplusOneBenchmarks>(args: args);

return;

static async Task CalibrateQueryCountsAsync()
{
    await using var postgres = await PostgresFixture.StartAsync();
    await DataSeeder.SeedAsync(postgres.ConnectionString);

    var counter = new QueryCounterInterceptor();

    var lazyOptions = new DbContextOptionsBuilder<OrdersDbContext>()
        .UseNpgsql(postgres.ConnectionString)
        .UseLazyLoadingProxies()
        .AddInterceptors(counter)
        .Options;

    var options = new DbContextOptionsBuilder<OrdersDbContext>()
        .UseNpgsql(postgres.ConnectionString)
        .AddInterceptors(counter)
        .Options;

    Console.WriteLine();
    Console.WriteLine("Scenario                           SQL queries");
    Console.WriteLine("---------------------------------- -----------");

    await Measure("Lazy loading (N+1)", lazyOptions, counter, async db =>
    {
        var orders = await db.Orders
            .OrderBy(o => o.Id).Take(NplusOneBenchmarks.OrdersToLoad).ToListAsync();
        foreach (var order in orders)
        foreach (var item in order.Items)
            _ = item.Product.Name;
    });

    await Measure("Eager loading (Include)", options, counter, async db =>
    {
        await db.Orders.AsNoTracking()
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .OrderBy(o => o.Id).Take(NplusOneBenchmarks.OrdersToLoad).ToListAsync();
    });

    await Measure("Split query", options, counter, async db =>
    {
        await db.Orders.AsNoTracking()
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .AsSplitQuery()
            .OrderBy(o => o.Id).Take(NplusOneBenchmarks.OrdersToLoad).ToListAsync();
    });

    await Measure("Projection (Select)", options, counter, async db =>
    {
        await db.Orders.AsNoTracking()
            .OrderBy(o => o.Id).Take(NplusOneBenchmarks.OrdersToLoad)
            .Select(o => new
            {
                o.Id,
                Items = o.Items.Select(i => new { i.ProductId, i.Product.Name, i.Quantity }).ToList()
            })
            .ToListAsync();
    });

    await Measure("Raw SQL (manual JOIN)", options, counter, async db =>
    {
        await db.Database.SqlQueryRaw<OrderItemFlatRow>(
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
                NplusOneBenchmarks.OrdersToLoad)
            .ToListAsync();
    });
}

static async Task Measure(
    string label,
    DbContextOptions<OrdersDbContext> options,
    QueryCounterInterceptor counter,
    Func<OrdersDbContext, Task> action)
{
    counter.Reset();
    await using var db = new OrdersDbContext(options);
    await action(db);
    Console.WriteLine($"{label,-34} {counter.Count,11}");
}