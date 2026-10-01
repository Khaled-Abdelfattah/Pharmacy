namespace PharmacySystem.Models.Entities;

public enum OrderStatus
{
    Pending = 0,
    Received = 1
}

public class Order
{
    public int Id { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime? ReceivedDate { get; set; }
    public string? Notes { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
