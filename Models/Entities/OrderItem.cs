namespace PharmacySystem.Models.Entities;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int QuantityOrdered { get; set; }
    public int? QuantityReceived { get; set; }

    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
