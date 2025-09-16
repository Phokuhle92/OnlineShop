namespace OnlineShop.API.Models.DTOs
{
    public class VerifyOnlyOtpDto
    {
        public string Email { get; set; } = string.Empty;
       public string OtpCode { get; set; } = string.Empty;
       //public string Role { get; set; } = string.Empty;
    }
}
