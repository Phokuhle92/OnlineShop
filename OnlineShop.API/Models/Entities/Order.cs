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

        public string Status { get; set; } = "Pending"; // e.g., Pending, Completed, Cancelled

        public decimal TotalAmount { get; set; }  // now writable, store total

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
