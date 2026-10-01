using Xunit;
using Microsoft.EntityFrameworkCore;
using PharmacySystem.Models.Entities;
using PharmacySystem.Models.ViewModels;
using PharmacySystem.Services;

namespace PharmacySystem.Tests;

public class ProductServiceTests
{
    [Fact]
    public async Task CreateProduct_SavesCorrectly()
    {
        using var db = TestDbFactory.Create();
        var service = new ProductService(db);

        var model = new ProductFormVM
        {
            Name = "Amoxicillin 500mg",
            CategoryId = 1,
            QuantityInStock = 50,
            PurchasePrice = 20m,
            SellingPrice = 30m,
            LowStockThreshold = 5
        };

        var product = await service.CreateAsync(model);

        Assert.True(product.Id > 0);
        Assert.Equal("Amoxicillin 500mg", product.Name);
        Assert.Equal(50, product.QuantityInStock);
    }

    [Fact]
    public async Task CreateProduct_ThrowsOnDuplicateBarcode()
    {
        using var db = TestDbFactory.Create();
        var service = new ProductService(db);

        var model1 = new ProductFormVM
        {
            Name = "Product A",
            Barcode = "1234567890",
            CategoryId = 1,
            QuantityInStock = 10,
            PurchasePrice = 5m,
            SellingPrice = 10m,
            LowStockThreshold = 3
        };

        await service.CreateAsync(model1);

        var model2 = new ProductFormVM
        {
            Name = "Product B",
            Barcode = "1234567890",  // same barcode
            CategoryId = 1,
            QuantityInStock = 5,
            PurchasePrice = 5m,
            SellingPrice = 10m,
            LowStockThreshold = 3
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(model2));
    }

    [Fact]
    public async Task GetByBarcode_ReturnsCorrectProduct()
    {
        using var db = TestDbFactory.Create();
        var p = TestDbFactory.SeedProduct(db);
        p.Barcode = "99887766";
        db.SaveChanges();

        var service = new ProductService(db);
        var found = await service.GetByBarcodeAsync("99887766");

        Assert.NotNull(found);
        Assert.Equal(p.Id, found!.Id);
    }

    [Fact]
    public async Task GetByBarcode_ReturnsNull_ForUnknownBarcode()
    {
        using var db = TestDbFactory.Create();
        var service = new ProductService(db);

        var result = await service.GetByBarcodeAsync("999999999999");
        Assert.Null(result);
    }

    [Fact]
    public async Task SearchAutocomplete_ReturnsMatchingProducts()
    {
        using var db = TestDbFactory.Create();
        var service = new ProductService(db);
        await service.CreateAsync(new ProductFormVM { Name = "Paracetamol 500mg", CategoryId = 1, QuantityInStock = 10, PurchasePrice = 5m, SellingPrice = 8m, LowStockThreshold = 3 });
        await service.CreateAsync(new ProductFormVM { Name = "Ibuprofen 400mg",   CategoryId = 1, QuantityInStock = 10, PurchasePrice = 5m, SellingPrice = 8m, LowStockThreshold = 3 });

        var results = await service.SearchAutocompleteAsync("para");

        Assert.Single(results);
        Assert.Contains("Paracetamol", results[0].Name);
    }

    [Fact]
    public async Task DeleteProduct_SoftDeletesOnly()
    {
        using var db = TestDbFactory.Create();
        var product = TestDbFactory.SeedProduct(db);
        var service = new ProductService(db);

        await service.DeleteAsync(product.Id);

        // Should not appear in normal queries (global filter)
        var visible = await service.GetAllAsync();
        Assert.DoesNotContain(visible, p => p.Id == product.Id);

        // But still exists in DB
        var raw = db.Products.IgnoreQueryFilters().Single(p => p.Id == product.Id);
        Assert.True(raw.IsDeleted);
    }

    [Fact]
    public async Task UpdateProduct_ClearsReorderFlag_WhenStockReplenished()
    {
        using var db = TestDbFactory.Create();
        var product = TestDbFactory.SeedProduct(db, qty: 0);
        // Manually add a reorder item
        db.ReorderItems.Add(new Models.Entities.ReorderItem { ProductId = product.Id, AddedAt = DateTime.UtcNow });
        db.SaveChanges();

        var service = new ProductService(db);
        await service.UpdateAsync(product.Id, new ProductFormVM
        {
            Name = product.Name,
            CategoryId = product.CategoryId,
            QuantityInStock = 20,         // restocked!
            PurchasePrice = product.PurchasePrice,
            SellingPrice = product.SellingPrice,
            LowStockThreshold = 5
        });

        var reorder = db.ReorderItems.Single(r => r.ProductId == product.Id);
        Assert.True(reorder.IsResolved);
    }
}
