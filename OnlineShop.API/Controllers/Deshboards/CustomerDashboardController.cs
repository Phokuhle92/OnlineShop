using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.API.Data;
using OnlineShop.API.Services;
using System.Threading.Tasks;

namespace OnlineShop.API.Controllers.Dashboards
{
    [ApiController]
    [Route("api/dashboard/customer")]
    [Authorize(Roles = "Customer")]
    public class CustomerDashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CustomerDashboardController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboard()
        {
            var userId = User.FindFirst("sub")?.Value;

            var userOrders = await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId)
                .ToListAsync();

            return Ok(new
            {
                totalOrders = userOrders.Count,
                totalSpent = userOrders.Sum(o => o.TotalAmount),
                recentOrders = userOrders
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .Select(o => new
                    {
                        o.Id,
                        o.Status,
                        o.TotalAmount,
                        o.OrderDate
                    })
            });
        }
    }
}
