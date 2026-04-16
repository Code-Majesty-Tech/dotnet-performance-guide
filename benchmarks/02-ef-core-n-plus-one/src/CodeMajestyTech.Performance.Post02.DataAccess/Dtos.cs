namespace CodeMajestyTech.Performance.Post02.DataAccess;

public sealed record OrderSummaryDto(
    int OrderId,
    string CustomerEmail,
    DateTime PlacedAt,
    decimal Total,
    List<OrderItemDto> Items);

public sealed record OrderItemDto(
    int ProductId,
    string ProductName,
    string ProductSku,
    int Quantity,
    decimal UnitPrice);

// Plain class (not record) so EF Core's SqlQueryRaw can populate it via
// settable properties matching the column names from the projected SQL.
public sealed class OrderItemFlatRow
{
    public int OrderId { get; set; }
    public string CustomerEmail { get; set; } = null!;
    public DateTime PlacedAt { get; set; }
    public decimal Total { get; set; }
    public int? ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductSku { get; set; }
    public int? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
}