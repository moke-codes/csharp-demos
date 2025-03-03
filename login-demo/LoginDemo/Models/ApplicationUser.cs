using Microsoft.AspNetCore.Identity;

namespace LoginDemo.Models;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public new bool TwoFactorEnabled { get; set; }
    public string? TwoFactorSecret { get; set; }
} 