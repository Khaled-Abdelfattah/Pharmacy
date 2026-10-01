using Xunit;
using Microsoft.EntityFrameworkCore;
using PharmacySystem.Models.Entities;
using PharmacySystem.Models.ViewModels;
using PharmacySystem.Services;

namespace PharmacySystem.Tests;

public class SalesServiceTests
{
    // ── CreateSaleAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task CreateSale_DeductsStock_Correctly()
    {
        using var db = TestDbFactory.Create();
        var product = TestDbFactory.SeedProduct(db, qty: 20, price: 10m);
        var service = new SalesService(db);

        var saleId = await service.CreateSaleAsync(new CreateSaleVM
        {
            Items = new() { new SaleLineVM { ProductId = product.Id, Quantity = 5 } }
        });

        var updatedProduct = await db.Products.FindAsync(product.Id);
        Assert.NotNull(updatedProduct);
        Assert.Equal(15, updatedProduct!.QuantityInStock);
        Assert.True(saleId > 0);
    }

    [Fact]
    public async Task CreateSale_RecordsSaleWithCorrectTotal()
    {
        using var db = TestDbFactory.Create();
        var product = TestDbFactory.SeedProduct(db, qty: 10, price: 20m);  // sell = 25
        var service = new SalesService(db);

        await service.CreateSaleAsync(new CreateSaleVM
        {
            Items = new() { new SaleLineVM { ProductId = product.Id, Quantity = 3 } }
        });

        var sale = db.Sales.Single();
        Assert.Equal(3 * 25m, sale.TotalAmount);
    }

    [Fact]
    public async Task CreateSale_ThrowsWhenInsufficientStock()
    {
        using var db = TestDbFactory.Create();
        var product = TestDbFactory.SeedProduct(db, qty: 2);
        var service = new SalesService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateSaleAsync(new CreateSaleVM
            {
                Items = new() { new SaleLineVM { ProductId = product.Id, Quantity = 5 } }
            })
        );
    }

    [Fact]
    public async Task CreateSale_ThrowsOnEmptyCart()
    {
        using var db = TestDbFactory.Create();
        var service = new SalesService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateSaleAsync(new CreateSaleVM { Items = new() })
        );
    }

    [Fact]
    public async Task CreateSale_AddsReorderItem_WhenStockHitsZero()
    {
        using var db = TestDbFactory.Create();
        var product = TestDbFactory.SeedProduct(db, qty: 3);
        var service = new SalesService(db);

        await service.CreateSaleAsync(new CreateSaleVM
        {
            Items = new() { new SaleLineVM { ProductId = product.Id, Quantity = 3 } }
        });

        var reorder = db.ReorderItems.SingleOrDefault(r => r.ProductId == product.Id);
        Assert.NotNull(reorder);
        Assert.False(reorder!.IsResolved);
    }

    [Fact]
    public async Task CreateSale_DoesNotDuplicateReorderItem()
    {
        using var db = TestDbFactory.Create();
        var product = TestDbFactory.SeedProduct(db, qty: 6);
        var service = new SalesService(db);

        // First sale: drains stock to 0
        await service.CreateSaleAsync(new CreateSaleVM
        {
            Items = new() { new SaleLineVM { ProductId = product.Id, Quantity = 6 } }
        });

        // Replenish manually so we can sell again
        var p = db.Products.Find(product.Id)!;
        p.QuantityInStock = 4;
        db.SaveChanges();

        // Second sale: drains to 0 again — should not create duplicate
        await service.CreateSaleAsync(new CreateSaleVM
        {
            Items = new() { new SaleLineVM { ProductId = product.Id, Quantity = 4 } }
        });

        var count = db.ReorderItems.Count(r => r.ProductId == product.Id);
        Assert.Equal(1, count);
    }

    // ── GetShiftReportAsync ─────────────────────────────────────────────

    [Fact]
    public async Task ShiftReport_FiltersCorrectly_ByDateRange()
    {
        using var db = TestDbFactory.Create();
        var product = TestDbFactory.SeedProduct(db, qty: 50);
        var service = new SalesService(db);

        var t1 = DateTime.UtcNow.AddHours(-5);
        var t2 = DateTime.UtcNow.AddHours(-1);

        // Create 2 sales inside the range
        db.Sales.Add(new Sale
        {
            SaleDate = t1,
            TotalAmount = 100m,
            SaleItems = new List<SaleItem> { new SaleItem { ProductId = product.Id, ProductName = product.Name, UnitPrice = 10m, Quantity = 10, LineTotal = 100m } }
        });
        db.Sales.Add(new Sale
        {
            SaleDate = t2,
            TotalAmount = 50m,
            SaleItems = new List<SaleItem> { new SaleItem { ProductId = product.Id, ProductName = product.Name, UnitPrice = 10m, Quantity = 5, LineTotal = 50m } }
        });
        // 1 sale outside the range
        db.Sales.Add(new Sale
        {
            SaleDate = DateTime.UtcNow.AddDays(-2),
            TotalAmount = 200m,
            SaleItems = new List<SaleItem> { new SaleItem { ProductId = product.Id, ProductName = product.Name, UnitPrice = 10m, Quantity = 20, LineTotal = 200m } }
        });
        db.SaveChanges();

        var report = await service.GetShiftReportAsync(t1.AddMinutes(-1), t2.AddMinutes(1));

        Assert.Equal(2, report.InvoiceCount);
        Assert.Equal(150m, report.TotalAmount);
        Assert.Equal(15, report.TotalUnitsSold);
    }

    // ── GetDailyInvoiceAsync ────────────────────────────────────────────

    [Fact]
    public async Task DailyInvoice_AggregatesByProductNameAndPrice()
    {
        using var db = TestDbFactory.Create();
        var product = TestDbFactory.SeedProduct(db, qty: 50, price: 20m);
        var service = new SalesService(db);

        var today = DateOnly.FromDateTime(DateTime.Today);
        var saleDate = today.ToDateTime(new TimeOnly(10, 0), DateTimeKind.Utc);

        // 2 separate sales of the same product today
        db.Sales.Add(new Sale
        {
            SaleDate = saleDate,
            TotalAmount = 50m,
            SaleItems = new List<SaleItem> { new SaleItem { ProductId = product.Id, ProductName = product.Name, UnitPrice = 25m, Quantity = 2, LineTotal = 50m } }
        });
        db.Sales.Add(new Sale
        {
            SaleDate = saleDate.AddHours(2),
            TotalAmount = 75m,
            SaleItems = new List<SaleItem> { new SaleItem { ProductId = product.Id, ProductName = product.Name, UnitPrice = 25m, Quantity = 3, LineTotal = 75m } }
        });
        db.SaveChanges();

        var invoice = await service.GetDailyInvoiceAsync(today);

        Assert.Single(invoice.Lines);               // grouped into one line
        Assert.Equal(5, invoice.Lines[0].TotalQuantity);
        Assert.Equal(125m, invoice.TotalAmount);
    }
}
