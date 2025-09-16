using OnlineShop.API.Models.DTOs.ProductDTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineShop.API.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductReadDto>> GetAllProductsAsync();
        Task<ProductReadDto?> GetProductByIdAsync(int id);
        //Task<IEnumerable<ProductReadDto>> SearchProductsByNameAsync(string name); // ✅ fixed
        Task<IEnumerable<ProductReadDto>> SearchProductsByNameAsync(string name, int page = 1, int pageSize = 10);

        // Updated to match DTO with IFormFile for image upload
        Task<ProductReadDto> CreateProductAsync(ProductCreateDto dto);
        Task<bool> UpdateProductAsync(int id, ProductUpdateDto dto);
        Task<bool> DeleteProductAsync(int id);
    }
}
