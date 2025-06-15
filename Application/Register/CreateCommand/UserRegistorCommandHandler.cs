using JWTAuthentication.Application.Abstractions.Interfaces;
using JWTAuthentication.Database;
using JWTAuthentication.Entities;
using JWTAuthentication.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace JWTAuthentication.Application.Register.CreateCommand;

public sealed class UserRegister(
    ApplicationDbContext context,
    UserManager<User> userManager,
    IPasswordHasher passwordHasher) : IRequestHandler<UserRegisterCommand, string>
{
    public async Task<string> HandleAsync(UserRegisterCommand request, CancellationToken cancellationToken)
    {
        // Optional: Check if user already exists
        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var user = new User
        {
            UserName = request.UserName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName,
            DateOfBirth = request.DateOfBirth,
            PasswordHash = passwordHasher.Generate(request.Password),
        };

        // Create user with password
        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        // Assign role
        var roleResult = await userManager.AddToRoleAsync(user, request.Role);
        if (roleResult.Succeeded) return user.Id;
        var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
        throw new ApplicationException($"Role assignment failed: {errors}");

    }
}