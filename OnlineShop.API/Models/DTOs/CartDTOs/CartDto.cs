using System.Collections.Generic;

namespace OnlineShop.API.Models.DTOs.CartDTOs
{
    public class CartDto
    {
        public string UserId { get; set; } = string.Empty;

        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();

        // Optional: total value of the cart
        public decimal TotalAmount => Items.Sum(i => i.Quantity * i.UnitPrice);
    }
}
