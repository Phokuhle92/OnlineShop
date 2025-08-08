using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineShop.API.Models.Entities
{
    public class CartItem
    {
        public int Id { get; set; }

        // Foreign key to Product
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        // Foreign key to User (customer)
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;

        public int Quantity { get; set; }

        // Optional: timestamp when item was added
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
