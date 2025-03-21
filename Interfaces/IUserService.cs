namespace JWTAuthentication.Interfaces;

public interface IUserService
{
    Task<string> Register(string username, string email, string password);
}