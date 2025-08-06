using System;

namespace OnlineShop.API.Models
{
    public class OtpEntry
    {
        public string Email { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;
        public DateTime ExpiryTime { get; set; }
        public bool IsVerified { get; set; }
    }
}
