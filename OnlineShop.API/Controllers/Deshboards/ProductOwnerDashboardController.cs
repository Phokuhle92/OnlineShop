using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.API.Data;
using OnlineShop.API.Services;
using System.Threading.Tasks;
using System.Linq;

namespace OnlineShop.API.Controllers.Dashboards
{
    [ApiController]
    [Route("api/dashboard/productowner")]
    [Authorize(Roles = "ProductOwner")]
    public class ProductOwnerDashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductOwnerDashboardController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboard()
        {
            var ownerId = User.FindFirst("sub")?.Value;

            var products = await _context.Products
                .Include(p => p.OrderItems)
                .Where(p => p.OwnerId == ownerId)
                .ToListAsync();

            var totalProducts = products.Count;
            var totalStock = products.Sum(p => p.Stock);
            var totalOrders = products.Sum(p => p.OrderItems.Count);
            var totalRevenue = products.Sum(p => p.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice));

            return Ok(new
            {
                totalProducts,
                totalStock,
                totalOrders,
                totalRevenue,
                topProducts = products
                    .OrderByDescending(p => p.OrderItems.Count)
                    .Take(5)
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        SoldCount = p.OrderItems.Count
                    })
            });
        }
    }
}
