using System;
using System.Collections.Generic;

namespace OnlineShop.API.Models.DTOs
{
    // Generic dashboard DTO
    public class DashboardDto
    {
        public int TotalUsers { get; set; }           // Only for Admin
        public int TotalProducts { get; set; }        // Admin/ProductOwner
        public int TotalOrders { get; set; }          // All roles
        public decimal TotalRevenue { get; set; }     // Admin/ProductOwner
        public decimal TotalSpent { get; set; }       // Customer only

        public List<MonthlyStats> MonthlyStats { get; set; } = new();
        public List<TopProduct> TopProducts { get; set; } = new();
        public List<RecentOrder> RecentOrders { get; set; } = new();
    }

    public class MonthlyStats
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Amount { get; set; }
    }

    public class TopProduct
    {
        public string Name { get; set; } = string.Empty;
        public int Sold { get; set; }
    }

    public class RecentOrder
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
