namespace JWTAuthentication.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(string username);
}