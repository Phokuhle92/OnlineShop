using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.API.Data;
using OnlineShop.API.Models;
using OnlineShop.API.Models.DTOs;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using OnlineShop.API.Models.DTOs.OrderDTOs;
using OnlineShop.API.Models.Entities;
using OnlineShop.API.Models.DTOs.CartDTOs;

namespace OnlineShop.API.Controllers.Dashboards
{
    [Route("api/dashboard/customer")]
    [ApiController]
    [Authorize(Roles = "Customer")]
    public class CustomerDashboardController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;

        public CustomerDashboardController(UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet("overview")]
        public async Task<IActionResult> GetCustomerDashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "Customer profile not found" });

            var profileDto = new CustomerProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Surname = user.Surname
            };

            // Load recent orders
            var recentOrders = await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Select(o => new OrderResponseDto
                {
                    OrderId = o.Id,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    TotalAmount = o.OrderItems.Sum(oi => oi.UnitPrice * oi.Quantity),
                    Items = o.OrderItems.Select(oi => new OrderItemDetailsDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Name,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice
                    }).ToList()
                })
                .ToListAsync();

            // Load cart items
            var cartItems = await _context.Set<CartItem>()
                .Where(ci => ci.UserId == userId)
                .Include(ci => ci.Product)
                .Select(ci => new CartItemDto
                {
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Name,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.Product.Price
                })
                .ToListAsync();

            var dashboardDto = new CustomerDashboardDto
            {
                Profile = profileDto,
                RecentOrders = recentOrders,
                CartItems = cartItems
            };

            return Ok(dashboardDto);
        }
    }
}
