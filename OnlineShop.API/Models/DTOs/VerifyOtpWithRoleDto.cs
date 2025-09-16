namespace OnlineShop.API.Models.DTOs
{
    public class VerifyOtpWithRoleDto
    {
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;   // Role must match the OTP type
        public string OtpCode { get; set; } = string.Empty;
    }
}
