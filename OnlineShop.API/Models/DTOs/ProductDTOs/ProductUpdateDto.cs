using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace OnlineShop.API.Models.DTOs.ProductDTOs
{
    public class ProductUpdateDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int Stock { get; set; }

        // Optional image
        public IFormFile? Image { get; set; }

        public int? CategoryId { get; set; }
    }
}
