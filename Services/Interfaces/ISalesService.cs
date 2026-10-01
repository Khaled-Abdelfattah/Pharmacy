using PharmacySystem.Models.ViewModels;

namespace PharmacySystem.Services.Interfaces;

public interface ISalesService
{
    /// <summary>Creates a sale with a DB transaction, deducts stock, triggers reorder if needed.</summary>
    Task<int> CreateSaleAsync(CreateSaleVM model);

    /// <summary>Returns sales log filtered by a date-time range for shift reporting.</summary>
    Task<ShiftReportVM> GetShiftReportAsync(DateTime from, DateTime to);

    /// <summary>Returns all items sold on a specific calendar date (for daily invoice).</summary>
    Task<DailyInvoiceVM> GetDailyInvoiceAsync(DateOnly date);
}
