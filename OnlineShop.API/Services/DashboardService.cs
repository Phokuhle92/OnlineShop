using Microsoft.EntityFrameworkCore;
using OnlineShop.API.Data;
using OnlineShop.API.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineShop.API.Services
{
    public class DashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        // -------------------- Monthly Revenue --------------------
        public async Task<List<MonthlyRevenueDto>> GetMonthlyAggregatedRevenueAsync()
        {
            return await _context.Orders
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new MonthlyRevenueDto
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Revenue = g.Sum(o => o.TotalAmount)
                })
                .OrderBy(r => r.Year).ThenBy(r => r.Month)
                .ToListAsync();
        }

        // -------------------- Top Products --------------------
        public async Task<List<TopProductDto>> GetTopProductsAsync()
        {
            return await _context.Products
                .Include(p => p.OrderItems) // Include navigation
                .Select(p => new TopProductDto
                {
                    ProductId = p.Id,
                    Name = p.Name ?? "", // fallback for null
                    SoldCount = p.OrderItems.Count() // EF handles empty collection as 0
                })
                .OrderByDescending(p => p.SoldCount)
                .Take(5)
                .ToListAsync();
        }


        // -------------------- Recent Orders --------------------
        public async Task<List<RecentOrderDto>> GetRecentOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .Select(o => new RecentOrderDto
                {
                    OrderId = o.Id,
                    CustomerName = o.User!.UserName ?? "", // null-forgiving + fallback
                    TotalAmount = o.TotalAmount,
                    Status = o.Status ?? "",
                    OrderDate = o.OrderDate
                })
                .ToListAsync();
        }
    }

    // -------------------- DTOs --------------------
    public class MonthlyRevenueDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Revenue { get; set; }
    }

    public class TopProductDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = "";
        public int SoldCount { get; set; }
    }

    public class RecentOrderDto
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; } = "";
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "";
        public DateTime OrderDate { get; set; }
    }
}
