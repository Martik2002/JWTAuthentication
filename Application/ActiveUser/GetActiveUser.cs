using JWTAuthentication.Application.Abstractions.Interfaces;
using JWTAuthentication.Common.Models.AuthResponse;
using JWTAuthentication.Entities;
using JWTAuthentication.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JWTAuthentication.Application.ActiveUser;

public class GetActiveUser(UserManager<User> userManager, IApplicationDbContext context, IUser user) : IRequestHandler<GetActiveUserCommand, ActiveUserResponse>
{
    public async Task<ActiveUserResponse> HandleAsync(GetActiveUserCommand request, CancellationToken cancellationToken)
    {
        if (user.Id == null)
         return null;

       

        var activeUser = await context.Users
            .SingleOrDefaultAsync(x => x.Id == user.Id, cancellationToken);
        var roles = await userManager.GetRolesAsync(activeUser);
        
        
        return new ActiveUserResponse
        {
            Id = activeUser.Id,
            UserName = activeUser.LastName,
            FirstName = activeUser.FirstName,
            LastName = activeUser.LastName,
            MiddleName = activeUser.MiddleName,
            DateOfBirth = activeUser.DateOfBirth,
            PhoneNumber = activeUser.PhoneNumber,
            Email = activeUser.Email,
            Roles = roles
        };
    }
}