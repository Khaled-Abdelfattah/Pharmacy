using Microsoft.EntityFrameworkCore;
using PharmacySystem.Data;
using PharmacySystem.Models.Entities;
using PharmacySystem.Models.ViewModels;
using PharmacySystem.Services.Interfaces;

namespace PharmacySystem.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _db;

    public OrderService(AppDbContext db) => _db = db;

    public async Task<List<OrderListItemVM>> GetAllAsync()
        => await _db.Orders
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new OrderListItemVM
            {
                Id = o.Id,
                SupplierName = o.SupplierName,
                OrderDate = o.OrderDate,
                Status = o.Status.ToString(),
                ItemCount = o.OrderItems.Count
            })
            .ToListAsync();

    public async Task<OrderDetailVM?> GetDetailAsync(int id)
    {
        var order = await _db.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return null;

        return new OrderDetailVM
        {
            Id = order.Id,
            SupplierName = order.SupplierName,
            OrderDate = order.OrderDate,
            Status = order.Status.ToString(),
            ReceivedDate = order.ReceivedDate,
            Notes = order.Notes,
            Items = order.OrderItems.Select(oi => new OrderDetailLineVM
            {
                OrderItemId = oi.Id,
                ProductId = oi.ProductId,
                ProductName = oi.Product.Name,
                CategoryName = oi.Product.Category.Name,
                QuantityOrdered = oi.QuantityOrdered,
                QuantityReceived = oi.QuantityReceived
            }).ToList()
        };
    }

    public async Task<List<ReorderNeedVM>> GetReorderNeedsAsync()
        => await _db.ReorderItems
            .Include(r => r.Product)
                .ThenInclude(p => p.Category)
            .Where(r => !r.IsResolved)
            .OrderBy(r => r.Product.Category.SortOrder)
            .ThenBy(r => r.Product.Name)
            .Select(r => new ReorderNeedVM
            {
                ProductId = r.ProductId,
                ProductName = r.Product.Name,
                CategoryName = r.Product.Category.Name,
                CategoryId = r.Product.CategoryId,
                AddedAt = r.AddedAt
            })
            .ToListAsync();

    public async Task<int> CreateAsync(CreateOrderVM model)
    {
        if (model.Items == null || !model.Items.Any(i => i.QuantityOrdered > 0))
            throw new InvalidOperationException("An order must have at least one item with quantity > 0.");

        var order = new Order
        {
            SupplierName = model.SupplierName,
            OrderDate = DateTime.UtcNow,
            Notes = model.Notes,
            Status = OrderStatus.Pending,
            OrderItems = model.Items
                .Where(i => i.QuantityOrdered > 0)
                .Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    QuantityOrdered = i.QuantityOrdered
                }).ToList()
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        // Mark included reorder items as resolved (they are now being ordered)
        var productIds = order.OrderItems.Select(oi => oi.ProductId).ToList();
        var reorders = await _db.ReorderItems
            .Where(r => productIds.Contains(r.ProductId) && !r.IsResolved)
            .ToListAsync();
        foreach (var r in reorders)
            r.IsResolved = true;

        await _db.SaveChangesAsync();
        return order.Id;
    }

    public async Task ReceiveOrderAsync(int orderId, List<ReceiveOrderItemVM> receivedItems)
    {
        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var order = await _db.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId)
                ?? throw new InvalidOperationException("Order not found.");

            if (order.Status == OrderStatus.Received)
                throw new InvalidOperationException("This order has already been received.");

            var productIds = order.OrderItems.Select(oi => oi.ProductId).ToList();
            var products = await _db.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            foreach (var received in receivedItems)
            {
                var orderItem = order.OrderItems.FirstOrDefault(oi => oi.Id == received.OrderItemId)
                    ?? throw new InvalidOperationException($"Order item ID {received.OrderItemId} not found.");

                if (received.QuantityReceived < 0)
                    throw new InvalidOperationException("Received quantity cannot be negative.");

                orderItem.QuantityReceived = received.QuantityReceived;

                // Add to product stock
                var product = products.First(p => p.Id == orderItem.ProductId);
                product.QuantityInStock += received.QuantityReceived;
                product.UpdatedAt = DateTime.UtcNow;

                // If product was in reorder list and now has stock, resolve it
                if (product.QuantityInStock > 0)
                {
                    var reorder = await _db.ReorderItems
                        .FirstOrDefaultAsync(r => r.ProductId == product.Id && !r.IsResolved);
                    if (reorder != null)
                        reorder.IsResolved = true;
                }
            }

            order.Status = OrderStatus.Received;
            order.ReceivedDate = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}
