using JWTAuthentication.Application.Abstractions.Interfaces;
using JWTAuthentication.Common.Results;
using JWTAuthentication.Entities;
using Microsoft.AspNetCore.Identity;

namespace JWTAuthentication.Application.Auth.Me;

public sealed record GetCurrentUserQuery : IRequest<Result<CurrentUserResponse>>;

public sealed record CurrentUserResponse(
    string Id,
    string UserName,
    string Email,
    string FirstName,
    string LastName,
    string MiddleName,
    string PhoneNumber,
    DateTime DateOfBirth,
    IReadOnlyList<string> Roles);

public sealed class GetCurrentUserQueryHandler(UserManager<User> userManager, IUser currentUser)
    : IRequestHandler<GetCurrentUserQuery, Result<CurrentUserResponse>>
{
    public async Task<Result<CurrentUserResponse>> HandleAsync(GetCurrentUserQuery request, CancellationToken cancellationToken)
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

        var roles = await userManager.GetRolesAsync(user);

        return new CurrentUserResponse(
            user.Id,
            user.UserName,
            user.Email,
            user.FirstName,
            user.LastName,
            user.MiddleName,
            user.PhoneNumber,
            user.DateOfBirth,
            roles.ToList());
    }
}
