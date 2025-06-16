using JWTAuthentication.Application.Abstractions.Interfaces;
using JWTAuthentication.Common.Models.AuthResponse;
using JWTAuthentication.Common.Services;
using JWTAuthentication.Database;
using JWTAuthentication.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JWTAuthentication.Application.Login;

public sealed class Login(ApplicationDbContext context, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService)
    : IRequestHandler<LoginCommand, JwtAuthResult>
{
    public async Task<JwtAuthResult> HandleAsync(LoginCommand request, CancellationToken cancellationToken = default)
    {
        if (request.Username == null || request.Password == null)
        {
            return null;
        }

        var user = await context.Users.SingleOrDefaultAsync(u => u.UserName == request.Username, cancellationToken);
        
        if (user == null)
        {
            return null;
        }

        if (!passwordHasher.Verify(request.Username, user.PasswordHash))
        {
            return null;
        }

        var jwtAuthResult = await jwtTokenService.GenerateToken(user);
        user.RefreshToken = jwtAuthResult.RefreshToken.TokenString;
        user.RefreshTokenExpiry = jwtAuthResult.RefreshToken.Expires;
        await context.SaveChangesAsync(cancellationToken);
        return jwtAuthResult;
    }
}