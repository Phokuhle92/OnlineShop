using System.ComponentModel.DataAnnotations;

namespace OnlineShop.API.Models.Entities
{
    public class Order
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "Pending";

        // ✅ This is required for the error to go away
        public List<OrderItem> OrderItems { get; set; } = new();
    }
}
