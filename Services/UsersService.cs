using JWTAuthentication.Entities;
using JWTAuthentication.Interfaces;

namespace JWTAuthentication.Services;

public class UsersService : IUserService
{
    private readonly IPasswordHasher _passwordHasher;   
    private readonly IApplicationDbContext _context;
    public UsersService(IPasswordHasher passwordHasher, IApplicationDbContext context)
    {
        _passwordHasher = passwordHasher;
        _context = context;
    }

    
    
    
    public async Task<string> Register(string username,string email, string password)
    {
        var hashedPassword = _passwordHasher.Generate(password);
        var user = new User
        {
            UserName = username,
            Email = email,
            PasswordHash = hashedPassword
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync(CancellationToken.None);
        return user.Id;
    }
}