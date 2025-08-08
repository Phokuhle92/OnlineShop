using System.Collections.Generic;

namespace OnlineShop.API.Models.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Navigation property for products in this category
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
