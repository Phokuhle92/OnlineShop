using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OnlineShop.API.Controllers.Dashboards
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/dashboard/admin")]
    public class AdminDashboardController : ControllerBase
    {
        [HttpGet("summary")]
        public IActionResult GetAdminSummary()
        {
            // Replace with actual logic: stats, analytics, etc.
            var summary = new
            {
                TotalUsers = 120,
                TotalOrders = 430,
                TotalProducts = 85,
                SystemHealth = "Good"
            };

            return Ok(summary);
        }
    }
}
