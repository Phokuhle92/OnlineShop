using OnlineShop.API.Models.DTOs.ProductDTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineShop.API.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductReadDto>> GetAllProductsAsync();
        Task<ProductReadDto?> GetProductByIdAsync(int id);
        Task<ProductReadDto> CreateProductAsync(ProductCreateDto dto);
        Task<bool> UpdateProductAsync(int id, ProductUpdateDto dto);
        Task<bool> DeleteProductAsync(int id);
    }
}
