using OnlineShop.API.Models.Entities;
using System;
using System.Collections.Generic;

namespace OnlineShop.API.Models.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }

        // Use this as the main stock property
        public int Stock { get; set; }

        public string ImageUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Optional flags
        public bool IsNewArrival { get; set; } = false;
        public bool IsDeal { get; set; } = false;
        public bool IsBestSeller { get; set; } = false;

        // Foreign key to Category
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }

        // Foreign key to Owner
        public string? OwnerId { get; set; }
        public ApplicationUser? Owner { get; set; }

        // Link to OrderItems
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
