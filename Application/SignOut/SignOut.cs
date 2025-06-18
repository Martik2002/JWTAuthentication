using JWTAuthentication.Application.Abstractions.Interfaces;
using JWTAuthentication.Entities;
using Microsoft.AspNetCore.Identity;

namespace JWTAuthentication.Application.SignOut;

public class SignOut(UserManager<User> userManager, SignInManager<User> signInManager, IUser user)
    : IRequestHandler<SignOutCommand, string>
{
    public async Task<string> HandleAsync(SignOutCommand request, CancellationToken cancellationToken = default)
    {
        var activeUser = await userManager.FindByIdAsync(user.Id);

        if (activeUser == null)
            return "User not found";

        await signInManager.SignOutAsync();

        return user.Id;
    }
}