using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OnlineShop.API.Controllers.Dashboards
{
    [Authorize(Roles = "Manager")]
    [ApiController]
    [Route("api/dashboard/manager")]
    public class ManagerDashboardController : ControllerBase
    {
        // Example: GET /api/dashboard/manager/summary
        [HttpGet("summary")]
        public IActionResult GetManagerSummary()
        {
            var summary = new
            {
                WelcomeMessage = "Welcome to the Manager Dashboard!",
                TotalUsers = 230,
                TotalRevenue = 325000.00,
                TotalOrders = 1270,
                PendingIssues = 12
            };

            return Ok(summary);
        }

        // Example: GET /api/dashboard/manager/users
        [HttpGet("users")]
        public IActionResult GetUserStats()
        {
            var users = new[]
            {
                new { Role = "Customer", Count = 150 },
                new { Role = "StoreUser", Count = 40 },
                new { Role = "ProductOwner", Count = 25 },
                new { Role = "Manager", Count = 15 }
            };

            return Ok(users);
        }

        // Example: GET /api/dashboard/manager/orders
        [HttpGet("orders")]
        public IActionResult GetOrderStats()
        {
            var stats = new
            {
                Completed = 980,
                Pending = 190,
                Cancelled = 100
            };

            return Ok(stats);
        }
    }
}
