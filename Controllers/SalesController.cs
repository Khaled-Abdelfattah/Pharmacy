using Microsoft.AspNetCore.Mvc;
using PharmacySystem.Models.ViewModels;
using PharmacySystem.Services.Interfaces;

namespace PharmacySystem.Controllers;

public class SalesController : Controller
{
    private readonly ISalesService _sales;
    private readonly IProductService _products;

    public SalesController(ISalesService sales, IProductService products)
    {
        _sales = sales;
        _products = products;
    }

    // GET /Sales — POS screen
    public IActionResult Index() => View();

    // GET /Sales/Autocomplete?term=...  — JSON for search
    [HttpGet]
    public async Task<IActionResult> Autocomplete(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return Json(Array.Empty<object>());
        var results = await _products.SearchAutocompleteAsync(term);
        return Json(results);
    }

    // GET /Sales/BarcodeCheck?barcode=...
    [HttpGet]
    public async Task<IActionResult> BarcodeCheck(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            return Json(new { found = false });
        var product = await _products.GetByBarcodeAsync(barcode.Trim());
        if (product == null)
            return Json(new { found = false, barcode });
        return Json(new
        {
            found = true,
            id = product.Id,
            name = product.Name,
            sellingPrice = product.SellingPrice,
            quantityInStock = product.QuantityInStock,
            categoryId = product.CategoryId,
            isDeficient = product.CategoryId == 2
        });
    }

    // POST /Sales/Create — submit the cart
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] CreateSaleVM model)
    {
        try
        {
            var saleId = await _sales.CreateSaleAsync(model);
            return Json(new { success = true, saleId });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    // GET /Sales/Log — Shift report screen
    public async Task<IActionResult> Log(DateTime? from, DateTime? to, string? preset)
    {
        var now = DateTime.UtcNow;
        var localNow = DateTime.Now;

        DateTime fromDt, toDt;
        switch (preset)
        {
            case "today":
                fromDt = DateTime.Today.ToUniversalTime();
                toDt = DateTime.Today.AddDays(1).AddSeconds(-1).ToUniversalTime();
                break;
            case "yesterday":
                fromDt = DateTime.Today.AddDays(-1).ToUniversalTime();
                toDt = DateTime.Today.AddSeconds(-1).ToUniversalTime();
                break;
            default:
                fromDt = from?.ToUniversalTime() ?? DateTime.Today.ToUniversalTime();
                toDt = to?.ToUniversalTime() ?? now;
                break;
        }

        var report = await _sales.GetShiftReportAsync(fromDt, toDt);
        ViewBag.FromLocal = fromDt.ToLocalTime();
        ViewBag.ToLocal = toDt.ToLocalTime();
        ViewBag.Preset = preset ?? "custom";
        return View(report);
    }
}
