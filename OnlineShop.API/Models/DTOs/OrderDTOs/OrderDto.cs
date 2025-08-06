namespace OnlineShop.API.Models.DTOs.OrderDTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }

        public List<OrderItemDetailsDto> Items { get; set; }
    }
}
