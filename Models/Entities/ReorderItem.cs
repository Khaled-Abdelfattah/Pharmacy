namespace PharmacySystem.Models.Entities;

public class ReorderItem
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    public bool IsResolved { get; set; }

    public Product Product { get; set; } = null!;
}
