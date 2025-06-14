using JWTAuthentication.Common.Models.AuthResponse;
using JWTAuthentication.Entities;

namespace JWTAuthentication.Interfaces;

public interface IJwtTokenService
{
    Task<JwtAuthResult> GenerateToken(User user);
}