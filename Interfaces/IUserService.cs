namespace JWTAuthentication.Interfaces;

public interface IUserService
{
    Task<string> LoginAsync(string username, string password);
    Task<string> Register(string username, string email, string password);
}