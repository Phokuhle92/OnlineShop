using System.Collections.Generic;
using System;

namespace OnlineShop.API.Models.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; } // ✅ added ImageUrl for frontend

        // Navigation property to products
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
