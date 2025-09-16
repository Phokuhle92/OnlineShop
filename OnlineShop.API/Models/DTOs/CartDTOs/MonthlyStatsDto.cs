namespace OnlineShop.API.Models.DTOs
{
    public class MonthlyStatsDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
    }
}
