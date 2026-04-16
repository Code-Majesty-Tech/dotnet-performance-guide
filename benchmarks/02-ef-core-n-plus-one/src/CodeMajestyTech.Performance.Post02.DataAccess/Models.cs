namespace CodeMajestyTech.Performance.Post02.DataAccess;

// Navigation properties are virtual to enable EF Core lazy-loading proxies
// for the LazyLoading_NPlus1 benchmark. Removing 'virtual' would silently
// disable lazy loading and make the N+1 scenario degenerate into eager loading.
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public virtual List<Product> Products { get; set; } = [];
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public virtual Category Category { get; set; } = null!;
}

public class Order
{
    public int Id { get; set; }
    public string CustomerEmail { get; set; } = null!;
    public DateTime PlacedAt { get; set; }
    public decimal Total { get; set; }
    public virtual List<OrderItem> Items { get; set; } = [];
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public virtual Order Order { get; set; } = null!;
    public int ProductId { get; set; }
    public virtual Product Product { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}