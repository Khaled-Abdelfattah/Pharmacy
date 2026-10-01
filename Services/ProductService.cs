using Microsoft.EntityFrameworkCore;
using PharmacySystem.Data;
using PharmacySystem.Models.Entities;
using PharmacySystem.Models.ViewModels;
using PharmacySystem.Services.Interfaces;

namespace PharmacySystem.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _db;

    public ProductService(AppDbContext db) => _db = db;

    public async Task<List<Product>> GetAllAsync(string? search = null, int? categoryId = null)
    {
        var query = _db.Products
            .Include(p => p.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(s)
                                  || (p.Barcode != null && p.Barcode.Contains(s)));
        }

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        return await query
            .OrderBy(p => p.Category.SortOrder)
            .ThenBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
        => await _db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Product?> GetByBarcodeAsync(string barcode)
        => await _db.Products.Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Barcode == barcode);

    public async Task<List<ProductAutocompleteItem>> SearchAutocompleteAsync(string term)
    {
        var t = term.Trim().ToLower();
        return await _db.Products
            .Include(p => p.Category)
            .Where(p => p.Name.ToLower().Contains(t)
                     || (p.Barcode != null && p.Barcode.Contains(t)))
            .OrderBy(p => p.Name)
            .Take(10)
            .Select(p => new ProductAutocompleteItem
            {
                Id = p.Id,
                Name = p.Name,
                Barcode = p.Barcode,
                SellingPrice = p.SellingPrice,
                QuantityInStock = p.QuantityInStock,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                IsDeficient = p.CategoryId == 2
            })
            .ToListAsync();
    }

    public async Task<Product> CreateAsync(ProductFormVM model)
    {
        await ValidateBarcodeUniqueness(model.Barcode, excludeId: null);

        var product = new Product
        {
            Name = model.Name,
            Barcode = string.IsNullOrWhiteSpace(model.Barcode) ? null : model.Barcode.Trim(),
            CategoryId = model.CategoryId,
            QuantityInStock = model.QuantityInStock,
            PurchasePrice = model.PurchasePrice,
            SellingPrice = model.SellingPrice,
            ExpiryDate = model.ExpiryDate,
            Notes = model.Notes,
            LowStockThreshold = model.LowStockThreshold,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return product;
    }

    public async Task UpdateAsync(int id, ProductFormVM model)
    {
        var product = await _db.Products.FindAsync(id)
            ?? throw new InvalidOperationException("Product not found.");

        await ValidateBarcodeUniqueness(model.Barcode, excludeId: id);

        var oldQty = product.QuantityInStock;

        product.Name = model.Name;
        product.Barcode = string.IsNullOrWhiteSpace(model.Barcode) ? null : model.Barcode.Trim();
        product.CategoryId = model.CategoryId;
        product.QuantityInStock = model.QuantityInStock;
        product.PurchasePrice = model.PurchasePrice;
        product.SellingPrice = model.SellingPrice;
        product.ExpiryDate = model.ExpiryDate;
        product.Notes = model.Notes;
        product.LowStockThreshold = model.LowStockThreshold;
        product.UpdatedAt = DateTime.UtcNow;

        // If stock was 0 and now > 0, resolve the reorder flag
        if (oldQty == 0 && model.QuantityInStock > 0)
        {
            var reorder = await _db.ReorderItems
                .FirstOrDefaultAsync(r => r.ProductId == id && !r.IsResolved);
            if (reorder != null)
                reorder.IsResolved = true;
        }

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _db.Products.FindAsync(id)
            ?? throw new InvalidOperationException("Product not found.");
        product.IsDeleted = true;
        product.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    // ── Helpers ────────────────────────────────────────────────────────────────
    private async Task ValidateBarcodeUniqueness(string? barcode, int? excludeId)
    {
        if (string.IsNullOrWhiteSpace(barcode)) return;
        var trimmed = barcode.Trim();
        var exists = await _db.Products.IgnoreQueryFilters()
            .AnyAsync(p => p.Barcode == trimmed && (!excludeId.HasValue || p.Id != excludeId.Value));
        if (exists)
            throw new InvalidOperationException($"Barcode '{trimmed}' is already assigned to another product.");
    }
}
