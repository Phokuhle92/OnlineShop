using OnlineShop.API.Models.DTOs.OrderDTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineShop.API.Interfaces
{
    public interface IOrderService
    {
        Task<OrderResponseDto> CreateOrderAsync(string userId, CreateOrderDto orderDto);
        Task<List<OrderResponseDto>> GetUserOrdersAsync(string userId);

        // Add these new methods:
        Task<OrderResponseDto> UpdateOrderAsync(string userId, int orderId, CreateOrderDto orderDto);
        Task DeleteOrderAsync(string userId, int orderId);
    }
}
