namespace OnlineShop.API.Models.DTOs.CartDTOs
{
    public class CartItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }

        // Add this property:
        public decimal UnitPrice { get; set; }
    }
}
