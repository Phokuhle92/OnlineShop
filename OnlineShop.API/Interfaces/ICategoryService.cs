using OnlineShop.API.Models.DTOs.CategoryDTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineShop.API.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryReadDto>> GetAllAsync();
        Task<CategoryReadDto?> GetByIdAsync(int id);
        Task<CategoryReadDto> CreateAsync(CategoryCreateDto dto);
        Task<bool> UpdateAsync(int id, CategoryCreateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
