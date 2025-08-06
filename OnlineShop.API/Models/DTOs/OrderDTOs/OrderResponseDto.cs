namespace OnlineShop.API.Models.DTOs.OrderDTOs
{
    public class OrderResponseDto
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public List<OrderItemDetailsDto> Items { get; set; } = new();
    }
}
