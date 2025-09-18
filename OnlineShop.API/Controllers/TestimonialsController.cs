using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.API.Data;
using OnlineShop.API.Models.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineShop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestimonialsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public TestimonialsController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: api/testimonials
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Testimonial>>> GetTestimonials()
        {
            return await _context.Testimonials
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        // GET: api/testimonials/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Testimonial>> GetTestimonial(int id)
        {
            var testimonial = await _context.Testimonials.FindAsync(id);
            if (testimonial == null) return NotFound();
            return testimonial;
        }

        // POST: api/testimonials
        [HttpPost]
        public async Task<ActionResult<Testimonial>> AddTestimonial([FromForm] string name, [FromForm] string feedback, [FromForm] IFormFile image)
        {
            var testimonial = new Testimonial
            {
                Name = name,
                Feedback = feedback,
                CreatedAt = DateTime.UtcNow
            };

            if (image != null)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "testimonials");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }

                testimonial.ImageUrl = $"/uploads/testimonials/{fileName}";
            }

            _context.Testimonials.Add(testimonial);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTestimonial), new { id = testimonial.Id }, testimonial);
        }

        // PUT: api/testimonials/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTestimonial(int id, [FromForm] string name, [FromForm] string feedback, [FromForm] IFormFile image)
        {
            var testimonial = await _context.Testimonials.FindAsync(id);
            if (testimonial == null) return NotFound();

            testimonial.Name = name;
            testimonial.Feedback = feedback;

            if (image != null)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "testimonials");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }

                testimonial.ImageUrl = $"/uploads/testimonials/{fileName}";
            }

            _context.Entry(testimonial).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Testimonials.Any(t => t.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/testimonials/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTestimonial(int id)
        {
            var testimonial = await _context.Testimonials.FindAsync(id);
            if (testimonial == null) return NotFound();

            // Optionally delete the image file
            if (!string.IsNullOrEmpty(testimonial.ImageUrl))
            {
                var filePath = Path.Combine(_env.WebRootPath, testimonial.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }

            _context.Testimonials.Remove(testimonial);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
