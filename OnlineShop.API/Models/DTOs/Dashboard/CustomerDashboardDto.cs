using OnlineShop.API.Models.DTOs.CartDTOs;
using OnlineShop.API.Models.DTOs.OrderDTOs;
using System.Collections.Generic;

namespace OnlineShop.API.Models.DTOs.Dashboard
{
    public class CustomerDashboardDto
    {
        public CustomerProfileDto Profile { get; set; } = new CustomerProfileDto();
        public List<OrderResponseDto> RecentOrders { get; set; } = new List<OrderResponseDto>();
        public List<CartItemDto> CartItems { get; set; } = new List<CartItemDto>();
    }
}
