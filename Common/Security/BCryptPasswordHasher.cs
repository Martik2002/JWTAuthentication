using JWTAuthentication.Entities;
using Microsoft.AspNetCore.Identity;

namespace JWTAuthentication.Common.Security;

/// <summary>
/// Plugs BCrypt into ASP.NET Core Identity, so UserManager/SignInManager
/// (password validation, lockout) work with the existing BCrypt hashes.
/// </summary>
public sealed class BCryptPasswordHasher : IPasswordHasher<User>
{
    public string HashPassword(User user, string password) =>
        BCrypt.Net.BCrypt.EnhancedHashPassword(password);

    public PasswordVerificationResult VerifyHashedPassword(User user, string hashedPassword, string providedPassword)
    {
        if (string.IsNullOrEmpty(hashedPassword) || string.IsNullOrEmpty(providedPassword))
        {
            return PasswordVerificationResult.Failed;
        }

        try
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(providedPassword, hashedPassword)
                ? PasswordVerificationResult.Success
                : PasswordVerificationResult.Failed;
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // Hash is not a BCrypt hash (corrupted or created by another hasher)
            return PasswordVerificationResult.Failed;
        }
    }
}
