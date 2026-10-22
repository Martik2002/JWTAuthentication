using JWTAuthentication.Application.Abstractions.Interfaces;
using JWTAuthentication.Common.Results;
using JWTAuthentication.Common.Security;
using JWTAuthentication.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JWTAuthentication.Application.Auth.Refresh;

public sealed class RefreshTokenCommandHandler(
    IApplicationDbContext context,
    IJwtTokenService tokenService,
    TimeProvider timeProvider,
    ILogger<RefreshTokenCommandHandler> logger) : IRequestHandler<RefreshTokenCommand, Result<AuthResponse>>
{
    public async Task<Result<AuthResponse>> HandleAsync(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = RefreshTokenHasher.Hash(request.RefreshToken);

        var user = await context.Users
            .SingleOrDefaultAsync(u => u.RefreshToken == tokenHash, cancellationToken);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (user is null || user.RefreshTokenExpiry is null || user.RefreshTokenExpiry <= now)
        {
            return AuthErrors.InvalidRefreshToken;
        }

        if (user.LockoutEnd is not null && user.LockoutEnd > timeProvider.GetUtcNow())
        {
            return AuthErrors.InvalidRefreshToken;
        }

        // Rotation: the old refresh token is replaced and can't be used again.
        var tokens = await tokenService.IssueTokensAsync(user);

        // ConcurrencyStamp is a concurrency token in Identity's model. Changing it makes EF Core
        // check the old value in the UPDATE, so if two requests use the same refresh token
        // at the same time, only the first one succeeds.
        user.ConcurrencyStamp = Guid.NewGuid().ToString();

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogWarning("Concurrent use of the same refresh token for user {UserId}", user.Id);
            return AuthErrors.InvalidRefreshToken;
        }

        return tokens;
    }
}
