using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OnlineShop.API.Controllers.Dashboards
{
    [Authorize(Roles = "StoreUser")]
    [ApiController]
    [Route("api/dashboard/storeuser")]
    public class StoreUserDashboardController : ControllerBase
    {
        // Example: GET /api/dashboard/storeuser/summary
        [HttpGet("summary")]
        public IActionResult GetStoreUserSummary()
        {
            var summary = new
            {
                WelcomeMessage = "Welcome to your Store User Dashboard!",
                ProductsManaged = 34,
                OrdersProcessed = 15,
                PendingRestocks = 4
            };

            return Ok(summary);
        }

        // Example: GET /api/dashboard/storeuser/products
        [HttpGet("products")]
        public IActionResult GetManagedProducts()
        {
            var products = new[]
            {
                new { ProductId = 1, Name = "Wireless Mouse", Stock = 120 },
                new { ProductId = 2, Name = "Laptop Stand", Stock = 45 }
            };

            return Ok(products);
        }

        // Example: GET /api/dashboard/storeuser/orders
        [HttpGet("orders")]
        public IActionResult GetProcessedOrders()
        {
            var orders = new[]
            {
                new { OrderId = 301, Status = "Completed", Total = 999.00 },
                new { OrderId = 302, Status = "Shipped", Total = 749.50 }
            };

            return Ok(orders);
        }
    }
}
