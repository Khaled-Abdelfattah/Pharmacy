using Microsoft.EntityFrameworkCore;
using PharmacySystem.Models.Entities;

namespace PharmacySystem.Data;

/// <summary>
/// Seeds sample products into the database if none exist yet.
/// Run at startup after migrations have been applied.
/// </summary>
public static class Seeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Only seed if there are no products at all
        if (await context.Products.IgnoreQueryFilters().AnyAsync())
            return;

        var products = new List<Product>
        {
            // ── Available Medicines (CategoryId = 1) ───────────────────────
            new()
            {
                Name = "Paracetamol 500mg",
                Barcode = "6221020050019",
                CategoryId = 1,
                QuantityInStock = 120,
                PurchasePrice = 8.00m,
                SellingPrice = 12.00m,
                ExpiryDate = new DateOnly(2026, 6, 30),
                LowStockThreshold = 10
            },
            new()
            {
                Name = "Amoxicillin 500mg Cap",
                Barcode = "6221020050026",
                CategoryId = 1,
                QuantityInStock = 60,
                PurchasePrice = 25.00m,
                SellingPrice = 35.00m,
                ExpiryDate = new DateOnly(2025, 12, 31),
                LowStockThreshold = 10
            },
            new()
            {
                Name = "Ibuprofen 400mg",
                Barcode = "6221020050033",
                CategoryId = 1,
                QuantityInStock = 80,
                PurchasePrice = 10.00m,
                SellingPrice = 15.00m,
                ExpiryDate = new DateOnly(2026, 3, 31),
                LowStockThreshold = 10
            },
            new()
            {
                Name = "Vitamin C 1000mg",
                Barcode = "6221020050040",
                CategoryId = 1,
                QuantityInStock = 3,   // Low stock to demo badge
                PurchasePrice = 18.00m,
                SellingPrice = 25.00m,
                ExpiryDate = new DateOnly(2026, 9, 30),
                LowStockThreshold = 5
            },
            new()
            {
                Name = "Omeprazole 20mg",
                Barcode = "6221020050057",
                CategoryId = 1,
                QuantityInStock = 0,   // Out of stock to demo badge + reorder
                PurchasePrice = 15.00m,
                SellingPrice = 22.00m,
                ExpiryDate = new DateOnly(2026, 1, 31),
                LowStockThreshold = 5
            },

            // ── Deficient Medicines (CategoryId = 2) ───────────────────────
            new()
            {
                Name = "Insulin Glargine 100IU/mL",
                Barcode = "6221020050064",
                CategoryId = 2,
                QuantityInStock = 8,
                PurchasePrice = 120.00m,
                SellingPrice = 150.00m,
                ExpiryDate = new DateOnly(2025, 8, 31),
                LowStockThreshold = 5,
                Notes = "Refrigerate. Prescription required."
            },
            new()
            {
                Name = "Methotrexate 2.5mg",
                Barcode = "6221020050071",
                CategoryId = 2,
                QuantityInStock = 4,
                PurchasePrice = 45.00m,
                SellingPrice = 65.00m,
                ExpiryDate = new DateOnly(2025, 10, 31),
                LowStockThreshold = 3,
                Notes = "Cytotoxic — handle with care."
            },

            // ── Personal Care Products (CategoryId = 3) ────────────────────
            new()
            {
                Name = "Cetaphil Gentle Cleanser 250mL",
                Barcode = "6221020050088",
                CategoryId = 3,
                QuantityInStock = 15,
                PurchasePrice = 85.00m,
                SellingPrice = 120.00m,
                LowStockThreshold = 5
            },
            new()
            {
                Name = "Nivea Body Lotion 400mL",
                Barcode = "6221020050095",
                CategoryId = 3,
                QuantityInStock = 20,
                PurchasePrice = 55.00m,
                SellingPrice = 80.00m,
                LowStockThreshold = 5
            },
            new()
            {
                Name = "Oral-B Toothpaste 100g",
                Barcode = "6221020050101",
                CategoryId = 3,
                QuantityInStock = 30,
                PurchasePrice = 20.00m,
                SellingPrice = 32.00m,
                LowStockThreshold = 5
            },
        };

        foreach (var p in products)
        {
            p.CreatedAt = DateTime.UtcNow;
            p.UpdatedAt = DateTime.UtcNow;
        }

        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        // Seed reorder entry for the out-of-stock product (Omeprazole)
        var outOfStock = products.First(p => p.QuantityInStock == 0);
        if (!await context.ReorderItems.AnyAsync(r => r.ProductId == outOfStock.Id))
        {
            context.ReorderItems.Add(new ReorderItem
            {
                ProductId = outOfStock.Id,
                AddedAt = DateTime.UtcNow,
                IsResolved = false
            });
            await context.SaveChangesAsync();
        }
    }
}
