using Microsoft.EntityFrameworkCore;
using OnlineShop.API.Data;
using OnlineShop.API.Interfaces;
using OnlineShop.API.Models;
using OnlineShop.API.Models.DTOs.CategoryDTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineShop.API.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;

        public CategoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CategoryReadDto>> GetAllAsync()
        {
            return await _context.Categories
                .Select(c => new CategoryReadDto
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();
        }

        public async Task<CategoryReadDto?> GetByIdAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return null;

            return new CategoryReadDto
            {
                Id = category.Id,
                Name = category.Name
            };
        }

        public async Task<CategoryReadDto> CreateAsync(CategoryCreateDto dto)
        {
            var category = new Category
            {
                Name = dto.Name
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return new CategoryReadDto
            {
                Id = category.Id,
                Name = category.Name
            };
        }

        public async Task<bool> UpdateAsync(int id, CategoryCreateDto dto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;

            category.Name = dto.Name;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
