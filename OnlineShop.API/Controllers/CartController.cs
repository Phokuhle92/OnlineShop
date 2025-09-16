using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.API.Data;
using OnlineShop.API.Models.Entities;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineShop.API.Controllers
{
    [Route("api/cart")]
    [ApiController]
    [Authorize(Roles = "Customer")]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return Ok(new { Items = new List<object>() });

            var result = new
            {
                UserId = cart.UserId,
                Items = cart.Items.Select(i => new
                {
                    i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    i.Quantity,
                    i.UnitPrice
                }).ToList()
            };

            return Ok(result);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (quantity <= 0)
                return BadRequest("Quantity must be greater than zero.");

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return NotFound("Product not found.");

            // Get or create cart
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId, Items = new List<CartItem>() };
                _context.Carts.Add(cart);
            }

            // Check if item exists
            var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = product.Price,
                    Cart = cart  // link properly to the Cart
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Item added to cart." });
        }

        [HttpPost("remove")]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return NotFound("Cart not found.");

            var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (cartItem == null)
                return NotFound("Item not found in cart.");

            cart.Items.Remove(cartItem);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Item removed from cart." });
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateCartItem(int productId, int quantity)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (quantity <= 0)
                return BadRequest("Quantity must be greater than zero.");

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return NotFound("Cart not found.");

            var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (cartItem == null)
                return NotFound("Item not found in cart.");

            cartItem.Quantity = quantity;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Cart item updated." });
        }
    }
}
