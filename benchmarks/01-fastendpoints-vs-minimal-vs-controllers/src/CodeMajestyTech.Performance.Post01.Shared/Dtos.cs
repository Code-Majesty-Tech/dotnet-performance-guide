namespace CodeMajestyTech.Performance.Post01.Shared;

public sealed record ProductResponse(
    int Id,
    string Name,
    string Sku,
    string? Description,
    decimal Price,
    int StockQuantity,
    string CategoryName,
    DateTime CreatedAt);

public sealed record CreateProductRequest(
    string Name,
    string Sku,
    string? Description,
    decimal Price,
    int StockQuantity,
    int CategoryId);

public sealed record UpdateProductRequest(
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity);

public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize);