using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.API.Data;
using OnlineShop.API.Models.DTOs.OrderDTOs;
using OnlineShop.API.Models.Entities;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
namespace OnlineShop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Customer")] // ONLY Customers allowed by default
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        // Customer creates order
        [HttpPost]
        public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated.");

            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
                return Unauthorized("User not found.");

            if (dto.Items == null || !dto.Items.Any())
                return BadRequest("No items in order.");

            var productIds = dto.Items.Select(i => i.ProductId).ToList();
            var products = await _context.Products
                                         .Where(p => productIds.Contains(p.Id))
                                         .ToListAsync();

            if (products.Count != dto.Items.Count)
                return BadRequest("One or more products not found.");

            var orderItems = new List<OrderItem>();
            decimal totalAmount = 0;

            foreach (var item in dto.Items)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product == null)
                    continue;

                if (product.Stock < item.Quantity)
                    return BadRequest($"Not enough stock for product {product.Name}");

                product.Stock -= item.Quantity;

                orderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price
                });

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

            return Ok(new
            {
                Message = "Order placed successfully",
                OrderId = order.Id,
                Total = order.TotalAmount
            });
        }

        // Customer gets own orders
        [HttpGet("my")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated.");

            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.OrderItems.Sum(oi => oi.UnitPrice * oi.Quantity),
                    Items = o.OrderItems.Select(oi => new OrderItemDetailsDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product != null ? oi.Product.Name : string.Empty,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice
                    }).ToList()
                })
                .ToListAsync();

            return Ok(orders);
        }
        // Customer updates order (only Pending)
        [HttpPut("{orderId}")]
        public async Task<IActionResult> UpdateOrder(int orderId, [FromBody] CreateOrderDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated.");

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
                return NotFound("Order not found.");

            if (order.Status != "Pending")
                return BadRequest("Only pending orders can be updated.");

            if (dto.Items == null || !dto.Items.Any())
                return BadRequest("No items in order.");

            // Restore stock from old order items
            foreach (var oldItem in order.OrderItems)
            {
                var product = await _context.Products.FindAsync(oldItem.ProductId);
                if (product != null)
                    product.Stock += oldItem.Quantity;
            }

            // Remove old order items
            _context.OrderItems.RemoveRange(order.OrderItems);

            var productIds = dto.Items.Select(i => i.ProductId).ToList();
            var products = await _context.Products
                                         .Where(p => productIds.Contains(p.Id))
                                         .ToListAsync();

            if (products.Count != dto.Items.Count)
                return BadRequest("One or more products not found.");

            var newOrderItems = new List<OrderItem>();
            decimal totalAmount = 0;

            foreach (var item in dto.Items)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product == null)
                    continue;

                if (product.Stock < item.Quantity)
                    return BadRequest($"Not enough stock for product {product.Name}");

                product.Stock -= item.Quantity;

                newOrderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price
                });

                totalAmount += product.Price * item.Quantity;
            }

            order.OrderItems = newOrderItems;
            order.TotalAmount = totalAmount;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Order updated successfully", order.Id, order.TotalAmount });
        }

        // Customer deletes order (only Pending)
        [HttpDelete("{orderId}")]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated.");

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
                return NotFound("Order not found.");

            if (order.Status != "Pending")
                return BadRequest("Only pending orders can be deleted.");

            // Restore stock
            foreach (var item in order.OrderItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                    product.Stock += item.Quantity;
            }

            _context.OrderItems.RemoveRange(order.OrderItems);
            _context.Orders.Remove(order);

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Order deleted successfully" });
        }

        // Admin or Manager: Get all orders (Only Admin and Manager)
        [HttpGet("all")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var result = orders.Select(o => new
            {
                o.Id,
                o.OrderDate,
                o.TotalAmount,
                o.Status,
                o.UserId,
                Items = o.OrderItems.Select(oi => new
                {
                    oi.ProductId,
                    ProductName = oi.Product.Name,
                    oi.Quantity,
                    oi.UnitPrice
                })
            });

            return Ok(result);
        }
    }
}
