namespace OnlineShop.API.Models.DTOs.OrderDTOs
{
    public class CreateOrderDto
    {
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

}
