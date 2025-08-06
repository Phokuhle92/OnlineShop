using OnlineShop.API.Data;
using OnlineShop.API.Models.DTOs.OrderDTOs;
using OnlineShop.API.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineShop.API.Interfaces
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;

        public OrderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<OrderResponseDto> CreateOrderAsync(string userId, CreateOrderDto orderDto)
        {
            // Your existing CreateOrderAsync implementation here...
            throw new NotImplementedException();
        }

        public async Task<List<OrderResponseDto>> GetUserOrdersAsync(string userId)
        {
            // Your existing GetUserOrdersAsync implementation here...
            throw new NotImplementedException();
        }

        public async Task<OrderResponseDto> UpdateOrderAsync(string userId, int orderId, CreateOrderDto orderDto)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
                throw new Exception("Order not found");

            // Clear existing order items and restore stock
            foreach (var existingItem in order.OrderItems)
            {
                var product = await _context.Products.FindAsync(existingItem.ProductId);
                if (product != null)
                    product.Stock += existingItem.Quantity;
            }
            _context.OrderItems.RemoveRange(order.OrderItems);

            order.OrderItems.Clear();

            // Add new items and reduce stock
            foreach (var item in orderDto.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null || product.Stock < item.Quantity)
                    throw new Exception("Invalid product or insufficient stock");

                product.Stock -= item.Quantity;

                order.OrderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price
                });
            }

            order.Status = "Updated";
            order.OrderDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new OrderResponseDto
            {
                OrderId = order.Id,
                OrderDate = order.OrderDate,
                Status = order.Status,
                Items = order.OrderItems.Select(i => new OrderItemDetailsDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name ?? "",
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };
        }

        public async Task DeleteOrderAsync(string userId, int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
                throw new Exception("Order not found");

            // Restore stock
            foreach (var item in order.OrderItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                    product.Stock += item.Quantity;
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
        }
    }
}
