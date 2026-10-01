using System.ComponentModel.DataAnnotations;

namespace PharmacySystem.Models.ViewModels;

// ── Product Autocomplete ───────────────────────────────────────────────────────
public class ProductAutocompleteItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public decimal SellingPrice { get; set; }
    public int QuantityInStock { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public bool IsDeficient { get; set; }
}

// ── Product Form (Create / Edit) ───────────────────────────────────────────────
public class ProductFormVM
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Product name is required.")]
    [StringLength(200)]
    [Display(Name = "Product Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "Barcode")]
    public string? Barcode { get; set; }

    [Required(ErrorMessage = "Please select a category.")]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative.")]
    [Display(Name = "Quantity in Stock")]
    public int QuantityInStock { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Purchase price must be positive.")]
    [Display(Name = "Purchase Price (ج.م)")]
    public decimal PurchasePrice { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Selling price must be positive.")]
    [Display(Name = "Selling Price (ج.م)")]
    public decimal SellingPrice { get; set; }

    [Display(Name = "Expiry Date")]
    public DateOnly? ExpiryDate { get; set; }

    [StringLength(500)]
    [Display(Name = "Notes")]
    public string? Notes { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Low-stock threshold must be at least 1.")]
    [Display(Name = "Low-Stock Alert (units)")]
    public int LowStockThreshold { get; set; } = 5;

    public List<CategoryOptionVM> Categories { get; set; } = new();
}

public class CategoryOptionVM
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

// ── Inventory List ─────────────────────────────────────────────────────────────
public class InventoryListVM
{
    public List<ProductRowVM> Products { get; set; } = new();
    public List<CategoryOptionVM> Categories { get; set; } = new();
    public string? Search { get; set; }
    public int? SelectedCategoryId { get; set; }
}

public class ProductRowVM
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public int QuantityInStock { get; set; }
    public decimal SellingPrice { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public int LowStockThreshold { get; set; }
    public StockStatus StockStatus { get; set; }
}

public enum StockStatus { Ok, Low, OutOfStock }

// ── Sales / POS ────────────────────────────────────────────────────────────────
public class CreateSaleVM
{
    public List<SaleLineVM> Items { get; set; } = new();
    public string? Notes { get; set; }
}

public class SaleLineVM
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

// ── Shift Report ───────────────────────────────────────────────────────────────
public class ShiftReportVM
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public int InvoiceCount { get; set; }
    public int TotalUnitsSold { get; set; }
    public decimal TotalAmount { get; set; }
    public List<ShiftSaleRowVM> Sales { get; set; } = new();
}

public class ShiftSaleRowVM
{
    public int SaleId { get; set; }
    public DateTime SaleDate { get; set; }
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
}

// ── Daily Invoice ──────────────────────────────────────────────────────────────
public class DailyInvoiceVM
{
    public DateOnly Date { get; set; }
    public int TotalInvoices { get; set; }
    public decimal TotalAmount { get; set; }
    public int TotalUnitsSold { get; set; }
    public List<DailyInvoiceLineVM> Lines { get; set; } = new();
}

public class DailyInvoiceLineVM
{
    public string ProductName { get; set; } = string.Empty;
    public int TotalQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}

// ── Orders ─────────────────────────────────────────────────────────────────────
public class OrderListItemVM
{
    public int Id { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ItemCount { get; set; }
}

public class OrderDetailVM
{
    public int Id { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? ReceivedDate { get; set; }
    public string? Notes { get; set; }
    public List<OrderDetailLineVM> Items { get; set; } = new();
}

public class OrderDetailLineVM
{
    public int OrderItemId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int QuantityOrdered { get; set; }
    public int? QuantityReceived { get; set; }
}

public class ReorderNeedVM
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public DateTime AddedAt { get; set; }
}

public class CreateOrderVM
{
    [Required(ErrorMessage = "Supplier name is required.")]
    [StringLength(200)]
    [Display(Name = "Supplier Name")]
    public string SupplierName { get; set; } = string.Empty;

    [Display(Name = "Notes")]
    [StringLength(500)]
    public string? Notes { get; set; }

    public List<CreateOrderItemVM> Items { get; set; } = new();
}

public class CreateOrderItemVM
{
    public int ProductId { get; set; }
    public int QuantityOrdered { get; set; }
}

public class ReceiveOrderItemVM
{
    public int OrderItemId { get; set; }
    public int QuantityReceived { get; set; }
}
