using PharmacySystem.Models.Entities;
using PharmacySystem.Models.ViewModels;

namespace PharmacySystem.Services.Interfaces;

public interface IOrderService
{
    Task<List<OrderListItemVM>> GetAllAsync();
    Task<OrderDetailVM?> GetDetailAsync(int id);
    Task<List<ReorderNeedVM>> GetReorderNeedsAsync();
    Task<int> CreateAsync(CreateOrderVM model);
    Task ReceiveOrderAsync(int orderId, List<ReceiveOrderItemVM> receivedItems);
}
