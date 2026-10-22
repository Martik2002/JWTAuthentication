using JWTAuthentication.Application.Auth;
using JWTAuthentication.Entities;

namespace JWTAuthentication.Interfaces;

public interface IJwtTokenService
{
    /// <summary>
    /// Creates a new access token and refresh token for the user.
    /// The refresh token hash and expiry are set on the user entity; the caller saves the changes.
    /// </summary>
    Task<AuthResponse> IssueTokensAsync(User user);
}
