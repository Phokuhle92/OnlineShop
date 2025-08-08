using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OnlineShop.API.Controllers.Dashboards
{
    [Authorize(Roles = "ProductOwner")]
    [ApiController]
    [Route("api/dashboard/productowner")]
    public class ProductOwnerDashboardController : ControllerBase
    {
        // Example: GET /api/dashboard/productowner/summary
        [HttpGet("summary")]
        public IActionResult GetProductOwnerSummary()
        {
            var summary = new
            {
                WelcomeMessage = "Welcome to your Product Owner Dashboard!",
                TotalProducts = 87,
                ProductsInStock = 65,
                TotalRevenue = 125000.75
            };

            return Ok(summary);
        }

        // Example: GET /api/dashboard/productowner/products
        [HttpGet("products")]
        public IActionResult GetOwnedProducts()
        {
            var products = new[]
            {
                new { ProductId = 1, Name = "Smartphone X1", Sales = 240 },
                new { ProductId = 2, Name = "Bluetooth Speaker", Sales = 102 }
            };

            return Ok(products);
        }

        // Example: GET /api/dashboard/productowner/sales
        [HttpGet("sales")]
        public IActionResult GetSalesSummary()
        {
            var sales = new
            {
                TotalOrders = 550,
                MonthlySales = 12000.00,
                BestSellingProduct = "Smartphone X1"
            };

            return Ok(sales);
        }
    }
}
