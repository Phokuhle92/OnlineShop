using Microsoft.EntityFrameworkCore;
using OnlineShop.API.Data;
using OnlineShop.API.Interfaces;
using OnlineShop.API.Models;
using OnlineShop.API.Models.DTOs.ProductDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineShop.API.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;

        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductReadDto>> GetAllAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Select(p => new ProductReadDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    ImageUrl = p.ImageUrl,
                    CreatedAt = p.CreatedAt,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.Name : null
                })
                .ToListAsync();
        }

        public async Task<ProductReadDto?> GetByIdAsync(int id)
        {
            var p = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (p == null) return null;

            return new ProductReadDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                ImageUrl = p.ImageUrl,
                CreatedAt = p.CreatedAt,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name
            };
        }

        public async Task<ProductReadDto> CreateAsync(ProductCreateDto dto)
        {
            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Stock = dto.Stock,
                ImageUrl = dto.ImageUrl,
                CategoryId = dto.CategoryId
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(product.Id) ?? throw new Exception("Failed to retrieve created product.");
        }

        public async Task<bool> UpdateAsync(int id, ProductUpdateDto dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.Stock = dto.Stock;
            product.ImageUrl = dto.ImageUrl;
            product.CategoryId = dto.CategoryId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
