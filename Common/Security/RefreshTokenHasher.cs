using System.Security.Cryptography;
using System.Text;

namespace JWTAuthentication.Common.Security;

/// <summary>
/// Refresh tokens are stored only as a SHA-256 hash, so a leaked database
/// does not give an attacker usable tokens. A fast hash is fine here (unlike
/// passwords) because the token is 64 random bytes and cannot be brute-forced.
/// </summary>
public static class RefreshTokenHasher
{
    public static string Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public static string Hash(string token)
    {
        ArgumentException.ThrowIfNullOrEmpty(token);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hash);
    }
}
