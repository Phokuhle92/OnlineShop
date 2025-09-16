using Microsoft.AspNetCore.Mvc;
using OnlineShop.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace OnlineShop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LandingController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LandingController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/landing/{userId}?search=&department=&badge=
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetLandingData(
            string userId,
            [FromQuery] string? search = null,
            [FromQuery] string? department = null,
            [FromQuery] string? badge = null) // badge: "NewArrivals,Deals,BestSellers"
        {
            // Fetch user info
            var user = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Surname,
                    u.UserName,
                    u.Role
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound(new { message = "User not found" });

            // Fetch categories
            var categories = await _context.Categories
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.ImageUrl
                })
                .ToListAsync();

            // Base product query
            var productsQuery = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            // Filter by search
            if (!string.IsNullOrEmpty(search))
                productsQuery = productsQuery.Where(p => p.Name.Contains(search));

            // Filter by department
            if (!string.IsNullOrEmpty(department) && department != "All Departments")
                productsQuery = productsQuery.Where(p => p.Category != null && p.Category.Name == department);

            // Filter by badges (support multiple)
            if (!string.IsNullOrEmpty(badge))
            {
                var badges = badge.Split(',', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries);

                if (badges.Length > 0)
                {
                    productsQuery = productsQuery.Where(p =>
                        (badges.Contains("NewArrivals") && p.IsNewArrival) ||
                        (badges.Contains("Deals") && p.IsDeal) ||
                        (badges.Contains("BestSellers") && p.IsBestSeller)
                    );
                }
            }

            var products = await productsQuery
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Price,
                    p.ImageUrl,
                    Category = p.Category == null ? null : new { p.Category.Id, p.Category.Name },
                    p.IsNewArrival,
                    p.IsDeal,
                    p.IsBestSeller
                })
                .ToListAsync();

            return Ok(new
            {
                user,
                categories,
                products
            });
        }
    }
}
