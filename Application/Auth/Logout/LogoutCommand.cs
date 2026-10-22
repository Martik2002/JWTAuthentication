using JWTAuthentication.Application.Abstractions.Interfaces;
using JWTAuthentication.Common.Results;
using JWTAuthentication.Entities;
using Microsoft.AspNetCore.Identity;

namespace JWTAuthentication.Application.Auth.Logout;

public sealed record LogoutCommand : IRequest<Result<bool>>;

/// <summary>
/// Revokes the user's refresh token. The current access token stays valid until it
/// expires (JWTs are stateless), which is why access tokens are short-lived.
/// </summary>
public sealed class LogoutCommandHandler(UserManager<User> userManager, IUser currentUser)
    : IRequestHandler<LogoutCommand, Result<bool>>
{
    public async Task<Result<bool>> HandleAsync(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(currentUser.Id))
        {
            return AuthErrors.UserNotFound;
        }

        var user = await userManager.FindByIdAsync(currentUser.Id);
        if (user is null)
        {
            return AuthErrors.UserNotFound;
        }

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Could not revoke refresh token: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        return true;
    }
}
