using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.API.Data;
using OnlineShop.API.Models.DTOs.Dashboard;

namespace OnlineShop.API.Controllers.Dashboards
{
    [ApiController]
    [Route("api/dashboard/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminDashboardController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<AdminDashboardDto>> GetDashboard()
        {
            // Total Sales
            var totalSales = await _context.Orders.SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            // Number of Orders
            var numberOfOrders = await _context.Orders.CountAsync();

            // Number of Customers
            var numberOfCustomers = await _context.Users.CountAsync(u => u.Role == "Customer");

            // Revenue This Month
            var revenueThisMonth = await _context.Orders
                .Where(o => o.OrderDate.Month == DateTime.Now.Month && o.OrderDate.Year == DateTime.Now.Year)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            // Sales Overview (last 6 months)
            var salesOverviewRaw = await _context.Orders
                .Where(o => o.OrderDate >= DateTime.Now.AddMonths(-6))
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalSales = g.Sum(o => o.TotalAmount)
                })
                .ToListAsync();

            var salesOverview = salesOverviewRaw
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .Select(g => new SalesOverviewDto
                {
                    Month = $"{g.Month}/{g.Year}", // formatting in memory
                    TotalSales = g.TotalSales
                })
                .ToList();

            // Order Distribution
            var orderDistribution = await _context.Orders
                .GroupBy(o => o.Status)
                .Select(g => new OrderDistributionDto
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            // Recent Orders
            var recentOrdersRaw = await _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .Include(o => o.User) // load the user (customer)
                .ToListAsync();

            var recentOrders = recentOrdersRaw
                .Select(o => new RecentOrderDto
                {
                    OrderId = o.Id,
                    Customer = o.CustomerName ?? (o.User != null ? $"{o.User.Name} {o.User.Surname}" : "Unknown"),
                    Total = o.TotalAmount
                })
                .ToList();

            // Low Stock Products
            var lowStockProducts = await _context.Products
                .Where(p => p.Stock < 10)
                .Select(p => new LowStockProductDto
                {
                    Product = p.Name,
                    Stock = p.Stock
                })
                .ToListAsync();

            // Return the complete dashboard
            return Ok(new AdminDashboardDto
            {
                TotalSales = (int)totalSales,
                NumberOfOrders = numberOfOrders,
                NumberOfCustomers = numberOfCustomers,
                RevenueThisMonth = revenueThisMonth,
                SalesOverview = salesOverview,
                OrderDistribution = orderDistribution,
                RecentOrders = recentOrders,
                LowStockProducts = lowStockProducts
            });
        }
    }
}
