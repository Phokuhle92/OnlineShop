namespace OnlineShop.API.Models.Entities
{
    public class CartItem
    {
        public int Id { get; set; }

        // Foreign Key to Cart
        public string CartId { get; set; } = null!;
        public Cart Cart { get; set; } = null!;

        // Foreign Key to Product
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
