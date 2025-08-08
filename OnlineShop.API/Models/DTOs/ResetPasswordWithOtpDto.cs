namespace OnlineShop.API.Models.DTOs
{ 
        public class ResetPasswordWithOtpDto
        {
            public string Email { get; set; }
            public string OtpCode { get; set; }
            public string NewPassword { get; set; }
        }
    
}
