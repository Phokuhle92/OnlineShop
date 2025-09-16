using System;
using System.Collections.Generic;

namespace OnlineShop.API.Models.Entities
{
    public class Order
    {
        public int Id { get; set; }

        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending";

        public decimal TotalAmount { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        // Optional: snapshot of name at the time of order
        public string CustomerName { get; set; } = string.Empty;
    }


}
