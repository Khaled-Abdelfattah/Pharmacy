using Microsoft.AspNetCore.Mvc;
using PharmacySystem.Services.Interfaces;

namespace PharmacySystem.Controllers;

public class InvoicesController : Controller
{
    private readonly ISalesService _sales;

    public InvoicesController(ISalesService sales) => _sales = sales;

    // GET /Invoices/Daily?date=2024-01-15
    public async Task<IActionResult> Daily(DateOnly? date)
    {
        var selectedDate = date ?? DateOnly.FromDateTime(DateTime.Today);
        var vm = await _sales.GetDailyInvoiceAsync(selectedDate);
        return View(vm);
    }

    // GET /Invoices/DailyJson?date=2024-01-15  — for client-side PDF/Excel export
    [HttpGet]
    public async Task<IActionResult> DailyJson(DateOnly date)
    {
        var vm = await _sales.GetDailyInvoiceAsync(date);
        return Json(vm);
    }
}
