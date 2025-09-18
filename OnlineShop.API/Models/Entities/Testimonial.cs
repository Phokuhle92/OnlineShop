using System;
using System.ComponentModel.DataAnnotations;

namespace OnlineShop.API.Models.Entities
{
    public class Testimonial
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required, MaxLength(1000)]
        public string Feedback { get; set; }

        [MaxLength(500)]
        public string ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
