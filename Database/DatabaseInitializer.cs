using JWTAuthentication.Common.Constants;
using JWTAuthentication.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JWTAuthentication.Database;

/// <summary>
/// Applies migrations, creates the roles and, only when configured, an admin user.
/// The admin password comes from configuration (Seed:AdminPassword), so no default
/// credentials exist in production.
/// </summary>
public sealed class DatabaseInitializer(
    ApplicationDbContext context,
    UserManager<User> userManager,
    RoleManager<IdentityRole> roleManager,
    IConfiguration configuration,
    ILogger<DatabaseInitializer> logger)
{
    public async Task InitialiseAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Applying database migrations...");
        await context.Database.MigrateAsync(cancellationToken);

        await SeedRolesAsync();
        await SeedAdminAsync();
    }

    private async Task SeedRolesAsync()
    {
        foreach (var role in new[] { Role.Admin, Role.User })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private async Task SeedAdminAsync()
    {
        var password = configuration["Seed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(password))
        {
            logger.LogInformation("Seed:AdminPassword is not set, skipping admin user seeding");
            return;
        }

        var userName = configuration["Seed:AdminUserName"] ?? "admin";
        if (await userManager.FindByNameAsync(userName) is not null)
        {
            return;
        }

        var admin = new User
        {
            UserName = userName,
            Email = configuration["Seed:AdminEmail"] ?? "admin@example.com",
            FirstName = "Admin",
            LastName = "Admin",
            DateOfBirth = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        };

        var result = await userManager.CreateAsync(admin, password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Could not seed admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        await userManager.AddToRoleAsync(admin, Role.Admin);
        logger.LogInformation("Seeded admin user {UserName}", userName);
    }
}
