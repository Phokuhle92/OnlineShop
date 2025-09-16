namespace OnlineShop.API.Models.DTOs
{
    public class VerifyLoginOtpDto
    {
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;
    }
}
