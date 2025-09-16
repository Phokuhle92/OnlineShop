namespace OnlineShop.API.Models
{
    public class OneTimePassword
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = default!;  // Add this navigation property

        public string Code { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime ExpiryTime { get; set; }
    }
}
