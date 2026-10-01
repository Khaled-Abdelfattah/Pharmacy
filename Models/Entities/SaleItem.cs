namespace PharmacySystem.Models.Entities;

public class SaleItem
{
    public int Id { get; set; }
    public int SaleId { get; set; }
    public int ProductId { get; set; }

    /// <summary>Snapshot of product name at time of sale — preserved even if product is renamed/deleted.</summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>Snapshot of unit price at time of sale.</summary>
    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }

    public Sale Sale { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
