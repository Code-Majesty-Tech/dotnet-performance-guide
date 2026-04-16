using Microsoft.EntityFrameworkCore;

namespace CodeMajestyTech.Performance.Post02.DataAccess;

public static class DataSeeder
{
    public const int CategoryCount = 50;
    public const int ProductCount = 1_000;
    public const int OrderCount = 10_000;
    public const int AverageItemsPerOrder = 5;

    public static async Task SeedAsync(string connectionString, CancellationToken ct = default)
    {
        var options = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        await using var db = new OrdersDbContext(options);
        await db.Database.EnsureCreatedAsync(ct);

        if (await db.Orders.AnyAsync(ct))
            return;

        var random = new Random(42); // deterministic seed for reproducibility

        var categories = Enumerable.Range(1, CategoryCount)
            .Select(i => new Category { Name = $"Category {i:D2}" })
            .ToList();
        db.Categories.AddRange(categories);
        await db.SaveChangesAsync(ct);

        var products = Enumerable.Range(1, ProductCount)
            .Select(i => new Product
            {
                Name = $"Product {i:D4}",
                Sku = $"SKU-{i:D6}",
                Price = Math.Round((decimal)(random.NextDouble() * 500 + 1), 2),
                CategoryId = categories[random.Next(categories.Count)].Id
            })
            .ToList();
        db.Products.AddRange(products);
        await db.SaveChangesAsync(ct);

        // Insert orders in batches so EF Core's change tracker doesn't balloon.
        const int batchSize = 500;
        for (var batchStart = 0; batchStart < OrderCount; batchStart += batchSize)
        {
            var batch = new List<Order>(batchSize);
            for (var i = 0; i < batchSize && batchStart + i < OrderCount; i++)
            {
                var orderIndex = batchStart + i + 1;
                var itemCount = 1 + random.Next(AverageItemsPerOrder * 2); // 1..2N

                var items = Enumerable.Range(0, itemCount)
                    .Select(_ =>
                    {
                        var product = products[random.Next(products.Count)];
                        return new OrderItem
                        {
                            ProductId = product.Id,
                            Quantity = 1 + random.Next(5),
                            UnitPrice = product.Price
                        };
                    })
                    .ToList();

                batch.Add(new Order
                {
                    CustomerEmail = $"customer{orderIndex:D5}@example.com",
                    PlacedAt = DateTime.UtcNow.AddMinutes(-random.Next(60 * 24 * 30)),
                    Total = items.Sum(it => it.Quantity * it.UnitPrice),
                    Items = items
                });
            }

            db.Orders.AddRange(batch);
            await db.SaveChangesAsync(ct);
            db.ChangeTracker.Clear();
        }
    }
}