using Microsoft.AspNetCore.Identity;
using OnlineShop.API.Models;
using OnlineShop.API.Models.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class ApplicationUser : IdentityUser
{
    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Surname { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Role { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<OneTimePassword> OneTimePasswords { get; set; } = new List<OneTimePassword>();

    // Add this property:
    public Cart? Cart { get; set; }   // <-- Needed for one-to-one mapping

    // Computed property
    public string FullName => $"{Name} {Surname}";
}
