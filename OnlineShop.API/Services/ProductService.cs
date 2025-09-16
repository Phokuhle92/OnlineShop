using Microsoft.EntityFrameworkCore;
using OnlineShop.API.Data;
using OnlineShop.API.Interfaces;
using OnlineShop.API.Models;
using OnlineShop.API.Models.DTOs.ProductDTOs;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OnlineShop.API.Models.Entities;
namespace OnlineShop.API.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProductService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetBaseUrl()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            return request != null ? $"{request.Scheme}://{request.Host}" : "";
        }

        public async Task<IEnumerable<ProductReadDto>> GetAllProductsAsync()
        {
            string baseUrl = GetBaseUrl();

            return await _context.Products
                .Include(p => p.Category)
                .Select(p => new ProductReadDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    ImageUrl = string.IsNullOrEmpty(p.ImageUrl) ? string.Empty : baseUrl + p.ImageUrl,
                    CreatedAt = p.CreatedAt,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.Name : string.Empty
                }).ToListAsync();
        }

        public async Task<ProductReadDto?> GetProductByIdAsync(int id)
        {
            string baseUrl = GetBaseUrl();

            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Id == id)
                .Select(p => new ProductReadDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    ImageUrl = string.IsNullOrEmpty(p.ImageUrl) ? string.Empty : baseUrl + p.ImageUrl,
                    CreatedAt = p.CreatedAt,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.Name : string.Empty
                }).FirstOrDefaultAsync();
        }

        public async Task<ProductReadDto> CreateProductAsync(ProductCreateDto dto)
        {
            string imageUrl = string.Empty;

            if (dto.Image != null && dto.Image.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid() + Path.GetExtension(dto.Image.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await dto.Image.CopyToAsync(stream);

                imageUrl = $"/uploads/{uniqueFileName}";
            }

            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Stock = dto.Stock,
                ImageUrl = imageUrl,
                CategoryId = dto.CategoryId
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            string baseUrl = GetBaseUrl();

            return new ProductReadDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                ImageUrl = string.IsNullOrEmpty(product.ImageUrl) ? string.Empty : baseUrl + product.ImageUrl,
                CreatedAt = product.CreatedAt,
                CategoryId = product.CategoryId,
                CategoryName = string.Empty
            };
        }
        public async Task<IEnumerable<ProductReadDto>> SearchProductsByNameAsync(string name, int page = 1, int pageSize = 10)
        {
            string baseUrl = GetBaseUrl();
            name = name.ToLower();

            var query = _context.Products
                .Include(p => p.Category)
                .Where(p =>
                    p.Name.ToLower().Contains(name) ||
                    p.Description.ToLower().Contains(name) ||
                    (p.Category != null && p.Category.Name.ToLower().Contains(name))
                )
                .Select(p => new ProductReadDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    ImageUrl = string.IsNullOrEmpty(p.ImageUrl) ? string.Empty : baseUrl + p.ImageUrl,
                    CreatedAt = p.CreatedAt,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.Name : string.Empty
                });

            // ✅ Pagination
            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> UpdateProductAsync(int id, ProductUpdateDto dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.Stock = dto.Stock;
            product.CategoryId = dto.CategoryId;

            if (dto.Image != null && dto.Image.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid() + Path.GetExtension(dto.Image.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await dto.Image.CopyToAsync(stream);

                product.ImageUrl = $"/uploads/{uniqueFileName}";
            }

            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
