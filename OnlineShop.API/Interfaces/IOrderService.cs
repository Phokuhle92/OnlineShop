using OnlineShop.API.Models.DTOs.OrderDTOs;
using OnlineShop.API.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineShop.API.Interfaces
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(string userId, CreateOrderDto dto);
        Task<List<Order>> GetUserOrdersAsync(string userId);
        Task<Order> UpdateOrderAsync(string userId, int orderId, CreateOrderDto dto);
        Task DeleteOrderAsync(string userId, int orderId);
        Task<List<Order>> GetAllOrdersAsync();
    }
}
