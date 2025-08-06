namespace OnlineShop.API.Models.DTOs
{
    public class RegisterDto
    {
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        // New field for role selection
        public string Role { get; set; } = "Customer"; // Default to "Customer"
    }
}
