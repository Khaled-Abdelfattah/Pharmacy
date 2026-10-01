using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacySystem.Data;
using PharmacySystem.Models.ViewModels;
using PharmacySystem.Services.Interfaces;

namespace PharmacySystem.Controllers;

public class OrdersController : Controller
{
    private readonly IOrderService _orders;
    private readonly IProductService _products;
    private readonly AppDbContext _db;

    public OrdersController(IOrderService orders, IProductService products, AppDbContext db)
    {
        _orders = orders;
        _products = products;
        _db = db;
    }

    // GET /Orders
    public async Task<IActionResult> Index()
    {
        var orders = await _orders.GetAllAsync();
        var reorderNeeds = await _orders.GetReorderNeedsAsync();
        ViewBag.ReorderNeeds = reorderNeeds;
        return View(orders);
    }

    // GET /Orders/Detail/{id}
    public async Task<IActionResult> Detail(int id)
    {
        var detail = await _orders.GetDetailAsync(id);
        if (detail == null) return NotFound();
        return View(detail);
    }

    // GET /Orders/Create
    public async Task<IActionResult> Create()
    {
        var products = await _products.GetAllAsync();
        var categories = await _db.Categories.OrderBy(c => c.SortOrder).ToListAsync();
        ViewBag.Products = products;
        ViewBag.Categories = categories;
        return View(new CreateOrderVM());
    }

    // POST /Orders/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateOrderVM model, string itemsJson)
    {
        // Parse items from JSON sent by JS (dynamic product list)
        if (!string.IsNullOrWhiteSpace(itemsJson))
        {
            try
            {
                var items = System.Text.Json.JsonSerializer.Deserialize<List<CreateOrderItemVM>>(itemsJson,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                model.Items = items ?? new();
            }
            catch
            {
                ModelState.AddModelError("", "Invalid item data.");
            }
        }

        if (!ModelState.IsValid || model.Items.Count == 0)
        {
            TempData["Error"] = "Please add at least one product to the order.";
            var products = await _products.GetAllAsync();
            var categories = await _db.Categories.OrderBy(c => c.SortOrder).ToListAsync();
            ViewBag.Products = products;
            ViewBag.Categories = categories;
            return View(model);
        }

        try
        {
            var id = await _orders.CreateAsync(model);
            TempData["Success"] = "Order created successfully.";
            return RedirectToAction(nameof(Detail), new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            var products = await _products.GetAllAsync();
            var categories = await _db.Categories.OrderBy(c => c.SortOrder).ToListAsync();
            ViewBag.Products = products;
            ViewBag.Categories = categories;
            return View(model);
        }
    }

    // POST /Orders/Receive/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Receive(int id, [FromForm] List<ReceiveOrderItemVM> receivedItems)
    {
        try
        {
            await _orders.ReceiveOrderAsync(id, receivedItems);
            TempData["Success"] = "Order marked as received. Stock has been updated.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Detail), new { id });
    }
}
