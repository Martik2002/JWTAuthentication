using JWTAuthentication.Application.Abstractions.Interfaces;
using JWTAuthentication.Database;
using JWTAuthentication.Entities;
using Microsoft.EntityFrameworkCore;

namespace JWTAuthentication.Application.ValidateRefreshToken;

public sealed class ValidateRefreshToken(ApplicationDbContext context)
    : IRequestHandler<ValidateRefreshTokenCommand, User>
{
    public async Task<User> HandleAsync(ValidateRefreshTokenCommand request,
        CancellationToken cancellationToken = default)
    {
        var user = await context.Users.SingleOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null || string.IsNullOrWhiteSpace(request.RefreshToken) ||
            user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiry <= DateTime.UtcNow)
        {
            return null;
        }

        return user;
    }
}