namespace OnlineShop.API.Models.DTOs.ProductDTOs
{
    public class ProductReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
    }
}
