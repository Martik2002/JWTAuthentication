using JWTAuthentication.Application.Abstractions.Interfaces;
using JWTAuthentication.Common.Results;
using JWTAuthentication.Entities;
using JWTAuthentication.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace JWTAuthentication.Application.Auth.Login;

public sealed class LoginCommandHandler(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    IJwtTokenService tokenService,
    IApplicationDbContext context,
    ILogger<LoginCommandHandler> logger) : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    public async Task<Result<AuthResponse>> HandleAsync(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByNameAsync(request.UserName);
        if (user is null)
        {
            logger.LogInformation("Login failed: unknown user {UserName}", request.UserName);
            return AuthErrors.InvalidCredentials;
        }

        // Verifies the password and counts failed attempts: after too many failures
        // the account is locked out for a while (protection against brute force).
        var signInResult = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!signInResult.Succeeded)
        {
            logger.LogWarning("Login failed for user {UserId}. Locked out: {IsLockedOut}",
                user.Id, signInResult.IsLockedOut);
            return AuthErrors.InvalidCredentials;
        }

        var tokens = await tokenService.IssueTokensAsync(user);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("User {UserId} logged in", user.Id);
        return tokens;
    }
}
