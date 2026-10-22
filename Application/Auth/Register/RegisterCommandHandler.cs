using JWTAuthentication.Application.Abstractions.Interfaces;
using JWTAuthentication.Common.Results;
using JWTAuthentication.Entities;
using Microsoft.AspNetCore.Identity;
using RoleNames = JWTAuthentication.Common.Constants.Role;

namespace JWTAuthentication.Application.Auth.Register;

public sealed class RegisterCommandHandler(
    UserManager<User> userManager,
    ILogger<RegisterCommandHandler> logger) : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
{
    public async Task<Result<RegisterResponse>> HandleAsync(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await userManager.FindByNameAsync(request.UserName) is not null)
        {
            return AuthErrors.UserNameTaken;
        }

        if (await userManager.FindByEmailAsync(request.Email) is not null)
        {
            return AuthErrors.EmailTaken;
        }

        var user = new User
        {
            UserName = request.UserName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName ?? string.Empty,
            // PostgreSQL "timestamp with time zone" only accepts UTC values
            DateOfBirth = DateTime.SpecifyKind(request.DateOfBirth!.Value.Date, DateTimeKind.Utc),
        };

        // UserManager hashes the password, validates it against the password policy
        // and fills normalized fields used for lookups (NormalizedUserName, NormalizedEmail).
        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return ToError(createResult);
        }

        var roleResult = await userManager.AddToRoleAsync(user, RoleNames.User);
        if (!roleResult.Succeeded)
        {
            // Should never happen: roles are seeded at startup. Fail loudly instead of
            // leaving a user without a role.
            throw new InvalidOperationException(
                $"Could not assign role '{RoleNames.User}': {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
        }

        logger.LogInformation("User {UserId} registered", user.Id);
        return new RegisterResponse(user.Id);
    }

    private static Error ToError(IdentityResult result)
    {
        var codes = result.Errors.Select(e => e.Code).ToList();
        if (codes.Contains(nameof(IdentityErrorDescriber.DuplicateUserName)))
        {
            return AuthErrors.UserNameTaken;
        }

        if (codes.Contains(nameof(IdentityErrorDescriber.DuplicateEmail)))
        {
            return AuthErrors.EmailTaken;
        }

        return Error.Validation(
            "Auth.InvalidRegistration",
            string.Join(" ", result.Errors.Select(e => e.Description)));
    }
}
