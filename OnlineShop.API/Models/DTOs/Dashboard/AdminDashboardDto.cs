namespace OnlineShop.API.Models.DTOs.Dashboard
{
    public class AdminDashboardDto
    {
        public int TotalSales { get; set; }
        public int NumberOfOrders { get; set; }
        public int NumberOfCustomers { get; set; }
        public decimal RevenueThisMonth { get; set; }

        // Graph Data
        public List<SalesOverviewDto> SalesOverview { get; set; } = new();
        public List<OrderDistributionDto> OrderDistribution { get; set; } = new();

        // Tables
        public List<RecentOrderDto> RecentOrders { get; set; } = new();
        public List<LowStockProductDto> LowStockProducts { get; set; } = new();
    }

    public class SalesOverviewDto
    {
        public string Month { get; set; } = string.Empty;
        public decimal TotalSales { get; set; }
    }

    public class OrderDistributionDto
    {
        public string Status { get; set; } = string.Empty;  // Pending, Completed, Cancelled
        public int Count { get; set; }
    }

    public class RecentOrderDto
    {
        public int OrderId { get; set; }
        public string Customer { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }

    public class LowStockProductDto
    {
        public string Product { get; set; } = string.Empty;
        public int Stock { get; set; }
    }
}
