using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.API.Data;
using System.Security.Claims;

namespace OnlineShop.API.Controllers.Dashboards
{
    [Authorize(Roles = "Customer")]
    [ApiController]
    [Route("api/dashboard/customer")]
    public class CustomerDashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CustomerDashboardController(AppDbContext context)
        {
            _context = context;
        }

        // GET /api/dashboard/customer/summary
        [HttpGet("summary")]
        public IActionResult GetCustomerSummary()
        {
            var dashboardSummary = new
            {
                WelcomeMessage = "Welcome to your customer dashboard!",
                TotalOrders = 5,
                PendingOrders = 2,
                RecentOrderDate = "2025-08-01"
            };

            return Ok(dashboardSummary);
        }

        // GET /api/dashboard/customer/orders
        [HttpGet("orders")]
        public IActionResult GetMyOrders()
        {
            var orders = new[]
            {
                new { OrderId = 101, Status = "Shipped", Total = 499.99 },
                new { OrderId = 102, Status = "Pending", Total = 259.00 }
            };

            return Ok(orders);
        }

        // GET /api/dashboard/customer/profile
        [HttpGet("profile")]
        public async Task<IActionResult> GetCustomerProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated.");

            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound("User not found.");

            var profile = new
            {
                Name = user.Name,
                Surname = user.Surname,
                Email = user.Email
            };

            return Ok(profile);
        }
    }
}
