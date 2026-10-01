using Microsoft.EntityFrameworkCore;
using PharmacySystem.Data;
using PharmacySystem.Models.Entities;
using PharmacySystem.Models.ViewModels;
using PharmacySystem.Services.Interfaces;

namespace PharmacySystem.Services;

public class SalesService : ISalesService
{
    private readonly AppDbContext _db;

    public SalesService(AppDbContext db) => _db = db;

    /// <summary>
    /// Creates a sale inside a DB transaction:
    ///   1. Validates stock for every line item
    ///   2. Deducts stock from each product
    ///   3. Inserts Sale + SaleItems rows
    ///   4. Adds ReorderItem if a product reaches 0
    /// </summary>
    public async Task<int> CreateSaleAsync(CreateSaleVM model)
    {
        if (model.Items == null || model.Items.Count == 0)
            throw new InvalidOperationException("Cannot create an empty sale.");

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var productIds = model.Items.Select(i => i.ProductId).Distinct().ToList();
            var products = await _db.Products
                .Include(p => p.Category)
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            // ── Validate all items before touching stock ────────────────────
            foreach (var line in model.Items)
            {
                var product = products.FirstOrDefault(p => p.Id == line.ProductId)
                    ?? throw new InvalidOperationException($"Product ID {line.ProductId} not found.");

                if (line.Quantity <= 0)
                    throw new InvalidOperationException($"Quantity for '{product.Name}' must be greater than zero.");

                if (product.QuantityInStock < line.Quantity)
                    throw new InvalidOperationException(
                        $"Insufficient stock for '{product.Name}'. " +
                        $"Available: {product.QuantityInStock}, Requested: {line.Quantity}.");
            }

            // ── Build Sale entity ───────────────────────────────────────────
            var sale = new Sale
            {
                SaleDate = DateTime.UtcNow,
                Notes = model.Notes,
                SaleItems = new List<SaleItem>()
            };

            foreach (var line in model.Items)
            {
                var product = products.First(p => p.Id == line.ProductId);
                var lineTotal = product.SellingPrice * line.Quantity;

                sale.SaleItems.Add(new SaleItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,   // snapshot
                    UnitPrice = product.SellingPrice, // snapshot
                    Quantity = line.Quantity,
                    LineTotal = lineTotal
                });

                product.QuantityInStock -= line.Quantity;
                product.UpdatedAt = DateTime.UtcNow;

                // ── Trigger auto-reorder if stock hits 0 ───────────────────
                if (product.QuantityInStock == 0)
                {
                    var alreadyQueued = await _db.ReorderItems
                        .AnyAsync(r => r.ProductId == product.Id && !r.IsResolved);
                    if (!alreadyQueued)
                    {
                        _db.ReorderItems.Add(new ReorderItem
                        {
                            ProductId = product.Id,
                            AddedAt = DateTime.UtcNow,
                            IsResolved = false
                        });
                    }
                }
            }

            sale.TotalAmount = sale.SaleItems.Sum(si => si.LineTotal);

            _db.Sales.Add(sale);
            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return sale.Id;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<ShiftReportVM> GetShiftReportAsync(DateTime from, DateTime to)
    {
        // Dates stored as UTC; convert to UTC if needed
        var sales = await _db.Sales
            .Include(s => s.SaleItems)
            .Where(s => s.SaleDate >= from && s.SaleDate <= to)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();

        return new ShiftReportVM
        {
            From = from,
            To = to,
            InvoiceCount = sales.Count,
            TotalAmount = sales.Sum(s => s.TotalAmount),
            TotalUnitsSold = sales.SelectMany(s => s.SaleItems).Sum(si => si.Quantity),
            Sales = sales.Select(s => new ShiftSaleRowVM
            {
                SaleId = s.Id,
                SaleDate = s.SaleDate,
                TotalAmount = s.TotalAmount,
                ItemCount = s.SaleItems.Sum(si => si.Quantity)
            }).ToList()
        };
    }

    public async Task<DailyInvoiceVM> GetDailyInvoiceAsync(DateOnly date)
    {
        var dayStart = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var dayEnd = date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        var sales = await _db.Sales
            .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
                    .ThenInclude(p => p.Category)
            .Where(s => s.SaleDate >= dayStart && s.SaleDate <= dayEnd)
            .ToListAsync();

        var allItems = sales.SelectMany(s => s.SaleItems).ToList();

        // Aggregate by product name + unit price
        var lines = allItems
            .GroupBy(si => new { si.ProductName, si.UnitPrice })
            .Select(g => new DailyInvoiceLineVM
            {
                ProductName = g.Key.ProductName,
                UnitPrice = g.Key.UnitPrice,
                TotalQuantity = g.Sum(si => si.Quantity),
                LineTotal = g.Sum(si => si.LineTotal),
                CategoryName = g.First().Product?.Category?.Name ?? "—"
            })
            .OrderBy(l => l.CategoryName)
            .ThenBy(l => l.ProductName)
            .ToList();

        return new DailyInvoiceVM
        {
            Date = date,
            TotalInvoices = sales.Count,
            TotalAmount = sales.Sum(s => s.TotalAmount),
            TotalUnitsSold = allItems.Sum(si => si.Quantity),
            Lines = lines
        };
    }
}
