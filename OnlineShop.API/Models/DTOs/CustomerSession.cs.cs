namespace OnlineShop.API.Models.DTOs
{
    public class CustomerSession
    {
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "Customer";
        public string Token { get; set; } = string.Empty;
        public DateTime LoginTime { get; set; } = DateTime.UtcNow;
    }
}
