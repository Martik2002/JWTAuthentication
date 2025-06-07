using JWTAuthentication.Entities;

namespace JWTAuthentication.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}