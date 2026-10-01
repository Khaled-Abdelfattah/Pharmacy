using Microsoft.EntityFrameworkCore;
using PharmacySystem.Data;
using PharmacySystem.Models.Entities;

namespace PharmacySystem.Tests;

/// <summary>
/// Creates a fresh in-memory SQLite database per test for isolation.
/// </summary>
public static class TestDbFactory
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        var db = new AppDbContext(options);
        db.Database.OpenConnection();
        db.Database.EnsureCreated();

        // Seed the 3 fixed categories (normally done via HasData migration)
        if (!db.Categories.Any())
        {
            db.Categories.AddRange(
                new Category { Id = 1, Name = "Available Medicines",   SortOrder = 1 },
                new Category { Id = 2, Name = "Deficient Medicines",   SortOrder = 2 },
                new Category { Id = 3, Name = "Personal Care Products", SortOrder = 3 }
            );
            db.SaveChanges();
        }

        return db;
    }

    public static Product SeedProduct(AppDbContext db, int categoryId = 1, int qty = 20, decimal price = 10m)
    {
        var p = new Product
        {
            Name = $"Test Product {Guid.NewGuid():N}",
            CategoryId = categoryId,
            QuantityInStock = qty,
            PurchasePrice = price,
            SellingPrice = price + 5,
            LowStockThreshold = 5,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Products.Add(p);
        db.SaveChanges();
        return p;
    }
}
