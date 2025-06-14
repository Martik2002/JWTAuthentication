using JWTAuthentication.Entities;

namespace JWTAuthentication.Interfaces;

public interface IJwtTokenService
{
    Task<string> GenerateToken(User user);
}