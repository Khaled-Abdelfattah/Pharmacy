using Microsoft.AspNetCore.Mvc;
using PharmacySystem.Models.Entities;
using PharmacySystem.Models.ViewModels;
using PharmacySystem.Services.Interfaces;
using PharmacySystem.Data;
using Microsoft.EntityFrameworkCore;

namespace PharmacySystem.Controllers;

public class InventoryController : Controller
{
    private readonly IProductService _products;
    private readonly AppDbContext _db;

    public InventoryController(IProductService products, AppDbContext db)
    {
        _products = products;
        _db = db;
    }

    // GET /Inventory
    public async Task<IActionResult> Index(string? search, int? categoryId)
    {
        var products = await _products.GetAllAsync(search, categoryId);
        var categories = await _db.Categories.OrderBy(c => c.SortOrder).ToListAsync();

        var vm = new InventoryListVM
        {
            Search = search,
            SelectedCategoryId = categoryId,
            Categories = categories.Select(c => new CategoryOptionVM { Id = c.Id, Name = c.Name }).ToList(),
            Products = products.Select(p => new ProductRowVM
            {
                Id = p.Id,
                Name = p.Name,
                Barcode = p.Barcode,
                CategoryName = p.Category.Name,
                CategoryId = p.CategoryId,
                QuantityInStock = p.QuantityInStock,
                SellingPrice = p.SellingPrice,
                ExpiryDate = p.ExpiryDate,
                LowStockThreshold = p.LowStockThreshold,
                StockStatus = p.QuantityInStock == 0
                    ? StockStatus.OutOfStock
                    : p.QuantityInStock <= p.LowStockThreshold
                        ? StockStatus.Low
                        : StockStatus.Ok
            }).ToList()
        };

        return View(vm);
    }

    // GET /Inventory/Create?barcode=...
    public async Task<IActionResult> Create(string? barcode)
    {
        var vm = new ProductFormVM
        {
            Barcode = barcode,
            Categories = await GetCategoryOptionsAsync()
        };
        return View(vm);
    }

    // POST /Inventory/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormVM model)
    {
        if (!ModelState.IsValid)
        {
            model.Categories = await GetCategoryOptionsAsync();
            return View(model);
        }

        try
        {
            await _products.CreateAsync(model);
            TempData["Success"] = $"Product '{model.Name}' added successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("Barcode", ex.Message);
            model.Categories = await GetCategoryOptionsAsync();
            return View(model);
        }
    }

    // GET /Inventory/Edit/{id}
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _products.GetByIdAsync(id);
        if (product == null) return NotFound();

        var vm = new ProductFormVM
        {
            Id = product.Id,
            Name = product.Name,
            Barcode = product.Barcode,
            CategoryId = product.CategoryId,
            QuantityInStock = product.QuantityInStock,
            PurchasePrice = product.PurchasePrice,
            SellingPrice = product.SellingPrice,
            ExpiryDate = product.ExpiryDate,
            Notes = product.Notes,
            LowStockThreshold = product.LowStockThreshold,
            Categories = await GetCategoryOptionsAsync()
        };

        return View(vm);
    }

    // POST /Inventory/Edit/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductFormVM model)
    {
        if (!ModelState.IsValid)
        {
            model.Categories = await GetCategoryOptionsAsync();
            return View(model);
        }

        try
        {
            await _products.UpdateAsync(id, model);
            TempData["Success"] = $"Product '{model.Name}' updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("Barcode", ex.Message);
            model.Categories = await GetCategoryOptionsAsync();
            return View(model);
        }
    }

    // POST /Inventory/Delete/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _products.DeleteAsync(id);
            TempData["Success"] = "Product removed from inventory.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<CategoryOptionVM>> GetCategoryOptionsAsync()
        => await _db.Categories
            .OrderBy(c => c.SortOrder)
            .Select(c => new CategoryOptionVM { Id = c.Id, Name = c.Name })
            .ToListAsync();
}
