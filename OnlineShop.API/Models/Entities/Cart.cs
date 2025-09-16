namespace OnlineShop.API.Models.Entities
{
    public class Cart
    {
        public string UserId { get; set; } = null!; // must be string
        public ApplicationUser User { get; set; } = null!;
        public List<CartItem> Items { get; set; } = new List<CartItem>();
    }

}
