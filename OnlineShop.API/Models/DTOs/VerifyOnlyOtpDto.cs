// Models/DTOs/VerifyOnlyOtpDto.cs
namespace OnlineShop.API.Models.DTOs
{
    public class VerifyOnlyOtpDto
    {
        public string Email { get; set; }
        public string OtpCode { get; set; }
    }
}
