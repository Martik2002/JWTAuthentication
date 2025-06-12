using JWTAuthentication.Entities;
using JWTAuthentication.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JWTAuthentication.Services;

public class UsersService : IUserService
{
    private readonly IPasswordHasher _passwordHasher; 
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IApplicationDbContext _context;
    public UsersService(IPasswordHasher passwordHasher, IApplicationDbContext context, IJwtTokenService jwtTokenService)
    {
        _passwordHasher = passwordHasher;
        _context = context;
        _jwtTokenService = jwtTokenService;
    }


    public async Task<string> LoginAsync(string username, string password)
    {
        if (username == null )
        {
            return null;
        }
        
        var user = await _context.Users.SingleOrDefaultAsync(u => u.UserName == username);
        if (user == null)
        {
            return null;
        }

        if (!_passwordHasher.Verify(password, user.PasswordHash))
        {
            return "Wrong password";
        }
        var token = _jwtTokenService.GenerateToken(user);
        return token;
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