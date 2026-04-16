using Microsoft.EntityFrameworkCore;

namespace CodeMajestyTech.Performance.Post01.Shared;

public sealed class ProductService(BenchmarkDbContext db)
{
    public async Task<ProductResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await db.Products
            .Where(p => p.Id == id)
            .Select(p => new ProductResponse(
                p.Id, p.Name, p.Sku, p.Description,
                p.Price, p.StockQuantity,
                p.Category.Name, p.CreatedAt))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PagedResponse<ProductResponse>> GetPagedAsync(int page, int pageSize,
        CancellationToken ct = default)
    {
        var query = db.Products.AsQueryable();
        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductResponse(
                p.Id, p.Name, p.Sku, p.Description,
                p.Price, p.StockQuantity,
                p.Category.Name, p.CreatedAt))
            .ToListAsync(ct);

        return new PagedResponse<ProductResponse>(items, totalCount, page, pageSize);
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken ct = default)
    {
        var product = new Product
        {
            Name = request.Name,
            Sku = request.Sku,
            Description = request.Description,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            CategoryId = request.CategoryId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Products.Add(product);
        await db.SaveChangesAsync(ct);

        return await GetByIdAsync(product.Id, ct)
               ?? throw new InvalidOperationException("Created product not found");
    }

    public async Task<ProductResponse?> UpdateAsync(int id, UpdateProductRequest request,
        CancellationToken ct = default)
    {
        var product = await db.Products.FindAsync(new object[] { id }, ct);
        if (product is null)
            return null;

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.StockQuantity = request.StockQuantity;
        product.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return await GetByIdAsync(id, ct);
    }
}