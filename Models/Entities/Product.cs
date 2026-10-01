namespace PharmacySystem.Models.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public int CategoryId { get; set; }
    public int QuantityInStock { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public string? Notes { get; set; }
    public int LowStockThreshold { get; set; } = 5;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }

    public Category Category { get; set; } = null!;
    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ReorderItem? ReorderItem { get; set; }
}
