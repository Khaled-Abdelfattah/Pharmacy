namespace PharmacySystem.Models.Entities;

public class Sale
{
    public int Id { get; set; }
    public DateTime SaleDate { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }

    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
}
