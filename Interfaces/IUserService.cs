using JWTAuthentication.Common.Models.AuthResponse;
using JWTAuthentication.Entities;

namespace JWTAuthentication.Interfaces;

public interface IUserService
{
    Task<JwtAuthResult> LoginAsync(string username, string password, CancellationToken cancellationToken );
    Task<User> ValidateRefreshTokenAsync(string userId, string refreshToken, CancellationToken cancellationToken );
    Task<string> Register(string username, string email, string password);
}