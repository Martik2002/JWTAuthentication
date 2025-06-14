using JWTAuthentication.Common.Models.AuthResponse;
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
    public async Task<JwtAuthResult> LoginAsync(string username, string password, CancellationToken cancellationToken)
    {
        if (username == null )
        {
            return null;
        }
        
        var user = await context.Users.SingleOrDefaultAsync(u => u.UserName == username, cancellationToken);
        if (user == null)
        {
            return null;
        }

        if (!passwordHasher.Verify(password, user.PasswordHash))
        {
            return null;
        }
        var token =  await jwtTokenService.GenerateToken(user);
        user.RefreshToken = token.RefreshToken.TokenString;
        user.RefreshTokenExpiry = token.RefreshToken.Expires;
        await context.SaveChangesAsync(cancellationToken);
        return token;
    }

    public async Task<User> ValidateRefreshTokenAsync(string userId, string refreshToken,
        CancellationToken cancellationToken)
    {
        var user = await context.Users.SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null || string.IsNullOrWhiteSpace(refreshToken) ||
            user.RefreshToken != refreshToken || user.RefreshTokenExpiry <= DateTime.UtcNow)
        {
            return null;
        }

        return user;
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