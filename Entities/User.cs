using Microsoft.AspNetCore.Identity;

namespace JWTAuthentication.Entities;

public class User : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    /// <summary>SHA-256 hash of the current refresh token (the token itself is never stored).</summary>
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime? RefreshTokenExpiry { get; set; }
    
}