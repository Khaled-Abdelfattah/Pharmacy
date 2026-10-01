using Xunit;
using Microsoft.EntityFrameworkCore;
using PharmacySystem.Models.Entities;
using PharmacySystem.Models.ViewModels;
using PharmacySystem.Services;

namespace PharmacySystem.Tests;

public class OrderServiceTests
{
    [Fact]
    public async Task CreateOrder_SavesOrderAndItems()
    {
        using var db = TestDbFactory.Create();
        var p1 = TestDbFactory.SeedProduct(db);
        var p2 = TestDbFactory.SeedProduct(db);
        var service = new OrderService(db);

        var orderId = await service.CreateAsync(new CreateOrderVM
        {
            SupplierName = "Test Supplier",
            Items = new()
            {
                new CreateOrderItemVM { ProductId = p1.Id, QuantityOrdered = 10 },
                new CreateOrderItemVM { ProductId = p2.Id, QuantityOrdered = 5 }
            }
        });

        var order = db.Orders.Find(orderId);
        Assert.NotNull(order);
        Assert.Equal("Test Supplier", order!.SupplierName);
        Assert.Equal(2, db.OrderItems.Count(oi => oi.OrderId == orderId));
    }

    [Fact]
    public async Task CreateOrder_ThrowsWhenNoItems()
    {
        using var db = TestDbFactory.Create();
        var service = new OrderService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateOrderVM { SupplierName = "Supplier", Items = new() })
        );
    }

    [Fact]
    public async Task CreateOrder_ResolvesReorderItemsForIncludedProducts()
    {
        using var db = TestDbFactory.Create();
        var p = TestDbFactory.SeedProduct(db, qty: 0);
        db.ReorderItems.Add(new Models.Entities.ReorderItem { ProductId = p.Id, AddedAt = DateTime.UtcNow });
        db.SaveChanges();

        var service = new OrderService(db);
        await service.CreateAsync(new CreateOrderVM
        {
            SupplierName = "Supplier",
            Items = new() { new CreateOrderItemVM { ProductId = p.Id, QuantityOrdered = 20 } }
        });

        var reorder = db.ReorderItems.Single(r => r.ProductId == p.Id);
        Assert.True(reorder.IsResolved);
    }

    [Fact]
    public async Task ReceiveOrder_UpdatesStockForAllItems()
    {
        using var db = TestDbFactory.Create();
        var p1 = TestDbFactory.SeedProduct(db, qty: 0);
        var p2 = TestDbFactory.SeedProduct(db, qty: 5);
        var service = new OrderService(db);

        var orderId = await service.CreateAsync(new CreateOrderVM
        {
            SupplierName = "Supplier",
            Items = new()
            {
                new CreateOrderItemVM { ProductId = p1.Id, QuantityOrdered = 30 },
                new CreateOrderItemVM { ProductId = p2.Id, QuantityOrdered = 20 }
            }
        });

        var orderItems = db.OrderItems.Where(oi => oi.OrderId == orderId).ToList();
        await service.ReceiveOrderAsync(orderId, orderItems.Select(oi => new ReceiveOrderItemVM
        {
            OrderItemId = oi.Id,
            QuantityReceived = oi.QuantityOrdered
        }).ToList());

        var updatedP1 = db.Products.Find(p1.Id);
        var updatedP2 = db.Products.Find(p2.Id);
        Assert.Equal(30, updatedP1!.QuantityInStock);   // 0 + 30
        Assert.Equal(25, updatedP2!.QuantityInStock);   // 5 + 20
    }

    [Fact]
    public async Task ReceiveOrder_MarksOrderAsReceived()
    {
        using var db = TestDbFactory.Create();
        var p = TestDbFactory.SeedProduct(db);
        var service = new OrderService(db);

        var orderId = await service.CreateAsync(new CreateOrderVM
        {
            SupplierName = "Supplier",
            Items = new() { new CreateOrderItemVM { ProductId = p.Id, QuantityOrdered = 10 } }
        });

        var orderItem = db.OrderItems.First(oi => oi.OrderId == orderId);
        await service.ReceiveOrderAsync(orderId, new List<ReceiveOrderItemVM>
        {
            new() { OrderItemId = orderItem.Id, QuantityReceived = 10 }
        });

        var order = db.Orders.Find(orderId);
        Assert.Equal(Models.Entities.OrderStatus.Received, order!.Status);
        Assert.NotNull(order.ReceivedDate);
    }

    [Fact]
    public async Task ReceiveOrder_ThrowsWhenAlreadyReceived()
    {
        using var db = TestDbFactory.Create();
        var p = TestDbFactory.SeedProduct(db);
        var service = new OrderService(db);

        var orderId = await service.CreateAsync(new CreateOrderVM
        {
            SupplierName = "Supplier",
            Items = new() { new CreateOrderItemVM { ProductId = p.Id, QuantityOrdered = 10 } }
        });

        var oi = db.OrderItems.First(x => x.OrderId == orderId);
        var receivePayload = new List<ReceiveOrderItemVM> { new() { OrderItemId = oi.Id, QuantityReceived = 10 } };

        await service.ReceiveOrderAsync(orderId, receivePayload);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ReceiveOrderAsync(orderId, receivePayload)
        );
    }

    [Fact]
    public async Task GetReorderNeeds_ReturnsOnlyUnresolved()
    {
        using var db = TestDbFactory.Create();
        var p1 = TestDbFactory.SeedProduct(db, qty: 0);
        var p2 = TestDbFactory.SeedProduct(db, qty: 0);
        db.ReorderItems.AddRange(
            new Models.Entities.ReorderItem { ProductId = p1.Id, AddedAt = DateTime.UtcNow, IsResolved = false },
            new Models.Entities.ReorderItem { ProductId = p2.Id, AddedAt = DateTime.UtcNow, IsResolved = true }
        );
        db.SaveChanges();

        var service = new OrderService(db);
        var needs = await service.GetReorderNeedsAsync();

        Assert.Single(needs);
        Assert.Equal(p1.Id, needs[0].ProductId);
    }
}
