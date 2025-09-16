using Microsoft.EntityFrameworkCore;
using OnlineShop.API.Data;
using OnlineShop.API.Models.DTOs.OrderDTOs;
using OnlineShop.API.Models.Entities;
using OnlineShop.API.Interfaces;
public class OrderService : IOrderService
{
    private readonly AppDbContext _context;

    public OrderService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Order> CreateOrderAsync(string userId, CreateOrderDto dto)
    {
        if (dto.Items == null || !dto.Items.Any())
            throw new ArgumentException("Order must have at least one item.");

        var productIds = dto.Items.Select(i => i.ProductId).ToList();
        var products = await _context.Products
                                     .Where(p => productIds.Contains(p.Id))
                                     .ToListAsync();

        if (products.Count != dto.Items.Count)
            throw new KeyNotFoundException("One or more products not found.");

        var orderItems = new List<OrderItem>();
        decimal totalAmount = 0m;

        foreach (var item in dto.Items)
        {
            var product = products.First(p => p.Id == item.ProductId);

            if (product.Stock < item.Quantity)
                throw new InvalidOperationException($"Not enough stock for product '{product.Name}'.");

            product.Stock -= item.Quantity;

            var orderItem = new OrderItem
            {
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = product.Price
            };

            orderItems.Add(orderItem);
            totalAmount += product.Price * item.Quantity;
        }

        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            TotalAmount = totalAmount,
            Status = "Pending",
            OrderItems = orderItems
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return order;
    }

    public async Task<List<Order>> GetUserOrdersAsync(string userId)
    {
        return await _context.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<Order> UpdateOrderAsync(string userId, int orderId, CreateOrderDto dto)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (order == null)
            throw new KeyNotFoundException("Order not found or does not belong to user.");

        if (dto.Items == null || !dto.Items.Any())
            throw new ArgumentException("Order must have at least one item.");

        // Restore stock for old order items
        foreach (var oldItem in order.OrderItems)
        {
            var product = await _context.Products.FindAsync(oldItem.ProductId);
            if (product != null)
            {
                product.Stock += oldItem.Quantity;
            }
        }

        _context.OrderItems.RemoveRange(order.OrderItems);
        await _context.SaveChangesAsync();

        var productIds = dto.Items.Select(i => i.ProductId).ToList();
        var products = await _context.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();

        if (products.Count != dto.Items.Count)
            throw new KeyNotFoundException("One or more products not found.");

        var newOrderItems = new List<OrderItem>();
        decimal totalAmount = 0m;

        foreach (var item in dto.Items)
        {
            var product = products.First(p => p.Id == item.ProductId);

            if (product.Stock < item.Quantity)
                throw new InvalidOperationException($"Not enough stock for product '{product.Name}'.");

            product.Stock -= item.Quantity;

            var orderItem = new OrderItem
            {
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = product.Price
            };

            newOrderItems.Add(orderItem);
            totalAmount += product.Price * item.Quantity;
        }

        order.OrderItems = newOrderItems;
        order.TotalAmount = totalAmount;
        order.OrderDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return order;
    }

    public async Task DeleteOrderAsync(string userId, int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (order == null)
            throw new KeyNotFoundException("Order not found or does not belong to user.");

        // Restore stock
        foreach (var item in order.OrderItems)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product != null)
            {
                product.Stock += item.Quantity;
            }
        }

        _context.OrderItems.RemoveRange(order.OrderItems);
        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Order>> GetAllOrdersAsync()
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }
}
