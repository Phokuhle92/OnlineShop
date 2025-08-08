using OnlineShop.API.Models.DTOs.CartDTOs;
using OnlineShop.API.Models.DTOs.OrderDTOs;
using System.Collections.Generic;

namespace OnlineShop.API.Models.DTOs
{
    public class CustomerDashboardDto
    {
        public CustomerProfileDto Profile { get; set; }
        public List<OrderResponseDto> RecentOrders { get; set; }
        public List<CartItemDto> CartItems { get; set; }
    }
}
