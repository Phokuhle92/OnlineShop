using OnlineShop.API.Models.DTOs.ProductDTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineShop.API.Interfaces
{
    public interface IProductService
    {
        Task<List<ProductReadDto>> GetAllAsync();
        Task<ProductReadDto?> GetByIdAsync(int id);
        Task<ProductReadDto> CreateAsync(ProductCreateDto dto);
        Task<bool> UpdateAsync(int id, ProductUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
