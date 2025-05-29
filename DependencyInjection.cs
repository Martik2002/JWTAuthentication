using JWTAuthentication.Database;
using JWTAuthentication.Interfaces;
using JWTAuthentication.Services;
using JWTAuthentication.Common.Services;
using Microsoft.EntityFrameworkCore;

namespace JWTAuthentication;

public static class DependencyInjection
{
    public static IServiceCollection ServiceProvider(this IServiceCollection service, IConfiguration configuration)
    {
        service.AddControllersWithViews();
        service.AddEndpointsApiExplorer();
        service.AddSwaggerGen();
        service.AddTransient<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        service.AddTransient<IUserService, UsersService>();
        service.AddTransient<IPasswordHasher, PasswordHasher>();
        service.AddControllers();
        service.AddDbContext<ApplicationDbContext>((options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });
        service.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();


        return service;
    }
}