using Microsoft.EntityFrameworkCore;

namespace CodeMajestyTech.Performance.Post01.Shared;

public static class DataSeeder
{
    public static async Task SeedAsync(string connectionString, CancellationToken ct = default)
    {
        var options = new DbContextOptionsBuilder<BenchmarkDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        await using var db = new BenchmarkDbContext(options);
        await db.Database.EnsureCreatedAsync(ct);

        if (await db.Categories.AnyAsync(ct))
            return;

        var categories = Enumerable.Range(1, 50)
            .Select(i => new Category
            {
                Name = $"Category {i:D2}",
                Description = $"Description for category {i}"
            })
            .ToList();

        db.Categories.AddRange(categories);
        await db.SaveChangesAsync(ct);

        var random = new Random(42); // deterministic seed for reproducibility
        var products = Enumerable.Range(1, 1000)
            .Select(i => new Product
            {
                Name = $"Product {i:D4}",
                Sku = $"SKU-{i:D6}",
                Description = $"Description for product {i}",
                Price = Math.Round((decimal)(random.NextDouble() * 500 + 1), 2),
                StockQuantity = random.Next(0, 10000),
                CategoryId = categories[random.Next(categories.Count)].Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            })
            .ToList();

        db.Products.AddRange(products);
        await db.SaveChangesAsync(ct);
    }
}