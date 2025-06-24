using JWTAuthentication.Common.Constants;
using JWTAuthentication.Entities;
using JWTAuthentication.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JWTAuthentication.Database;

public class IdentityDataInitializer(
    UserManager<User> userManager,
    RoleManager<IdentityRole> roleManager,
    IPasswordHasher passwordHasher,
    IApplicationDbContext context)
{
    public async Task InitialiseAsync(WebApplication app)
    {
        try
        {
            await DatabaseInitialiseAsync(app);
            await SeedRolesAsync();
            await SeedUsersAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    private async Task DatabaseInitialiseAsync(WebApplication app)
    {
        Console.WriteLine("Initialising database...");
        using var scope = app.Services.CreateScope();
    //    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        //await db.Database.MigrateAsync(); // Applies migrations automatically
        Console.WriteLine("Migrated database");
    }

    private async Task SeedRolesAsync()
    {
        var roles = new List<string>
        {
            Role.Admin,
            Role.User,
        };

        foreach (var role in roles.Where(role => !roleManager.Roles.Any(r => r.Name == role)))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }


    private async Task SeedUsersAsync()
    {
        var user = new User
        {
            UserName = "admin",
            Email = "admin@gmail.com",
            FirstName = "Admin",
            LastName = "Admin",
            MiddleName = "Admin",
            DateOfBirth = DateTime.UtcNow,
            PasswordHash = passwordHasher.Generate("admin"),
            PhoneNumber = "123456789",
        };

        if (!context.Users.Any(x => x.UserName == user.UserName))
        {
            context.Users.Add(user);
            await context.SaveChangesAsync(CancellationToken.None);
            await userManager.AddToRoleAsync(user, Role.Admin);
        }
    }
}