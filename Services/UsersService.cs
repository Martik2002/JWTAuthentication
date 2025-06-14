using JWTAuthentication.Entities;
using JWTAuthentication.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JWTAuthentication.Services;

public class UsersService(
    IPasswordHasher passwordHasher,
    IApplicationDbContext context,
    IJwtTokenService jwtTokenService)
    : IUserService
{
    public async Task<string> LoginAsync(string username, string password)
    {
        if (username == null )
        {
            return null;
        }
        
        var user = await context.Users.SingleOrDefaultAsync(u => u.UserName == username);
        if (user == null)
        {
            return null;
        }

        if (!passwordHasher.Verify(password, user.PasswordHash))
        {
            return "Wrong password";
        }
        var token =  await jwtTokenService.GenerateToken(user);
        return token;
    }

    public async Task<string> Register(string username,string email, string password)
    {
        var hashedPassword = passwordHasher.Generate(password);
        var user = new User
        {
            UserName = username,
            Email = email,
            PasswordHash = hashedPassword
        };
        context.Users.Add(user);
        await context.SaveChangesAsync(CancellationToken.None);
        return user.Id;
    }
}