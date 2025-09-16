namespace OnlineShop.API.Models.DTOs
{
    public class ResetPasswordWithOtpDto
    {
        public string Email { get; set; } = string.Empty;
       // public string OtpCode { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
