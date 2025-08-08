using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;

    // Optional: For clarity, you could store the user's default role or dashboard type
    public string Role { get; set; } = string.Empty;
}
