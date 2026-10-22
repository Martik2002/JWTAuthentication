using System.Text;
using System.Threading.RateLimiting;
using FluentValidation;
using JWTAuthentication.Application.Abstractions.Interfaces;
using JWTAuthentication.Application.Abstractions.Mediator;
using JWTAuthentication.Application.Abstractions.Services;
using JWTAuthentication.Common.Errors;
using JWTAuthentication.Common.Models;
using JWTAuthentication.Common.Security;
using JWTAuthentication.Common.Services;
using JWTAuthentication.Database;
using JWTAuthentication.Endpoints;
using JWTAuthentication.Entities;
using JWTAuthentication.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace JWTAuthentication;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddHttpContextAccessor();
        services.AddScoped<IUser, CurrentUser>();
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<DatabaseInitializer>();

        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddValidatorsFromAssemblyContaining(typeof(DependencyInjection), includeInternalTypes: true);

        services.Scan(scan => scan
            .FromAssembliesOf(typeof(DependencyInjection))
            .AddClasses(classes => classes.AssignableTo(typeof(IRequestHandler<,>)), publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services
            .AddDatabase(configuration)
            .AddIdentityServices()
            .AddJwtAuthentication(configuration)
            .AddAuthRateLimiting(configuration)
            .AddSwagger();

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();

        return services;
    }

    private static IServiceCollection AddIdentityServices(this IServiceCollection services)
    {
        // Registered before AddIdentityCore, which only adds its default hasher if none exists
        services.AddScoped<IPasswordHasher<User>, BCryptPasswordHasher>();

        // AddIdentityCore instead of AddIdentity: this is a JWT API, so Identity's cookie
        // authentication is not needed.
        services.AddIdentityCore<User>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager();

        return services;
    }

    private static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        var jwtKey = configuration["JwtSettings:Key"];
        // HMAC-SHA512 needs a key of at least 64 bytes
        if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 64)
        {
            throw new InvalidOperationException(
                "JwtSettings:Key is missing or shorter than 64 characters. " +
                "Set it with 'dotnet user-secrets set \"JwtSettings:Key\" \"<value>\"' " +
                "or the 'JwtSettings__Key' environment variable.");
        }

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = configuration["JwtSettings:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = configuration["JwtSettings:Audience"],
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha512],
                    // Default is 5 minutes, too generous for short-lived access tokens
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        services.AddAuthorization();
        return services;
    }

    private static IServiceCollection AddAuthRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var permitLimit = configuration.GetValue("RateLimiting:AuthPermitLimit", 10);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Limits login/register/refresh attempts per client IP to slow down brute force attacks
            options.AddPolicy(AuthEndpoints.RateLimitPolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = permitLimit,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));
        });

        return services;
    }

    private static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "JWT Authentication API", Version = "v1" });

            var bearerScheme = new OpenApiSecurityScheme
            {
                Description = "Paste the access token (without the 'Bearer ' prefix).",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Id = JwtBearerDefaults.AuthenticationScheme,
                    Type = ReferenceType.SecurityScheme
                }
            };

            c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, bearerScheme);
            c.AddSecurityRequirement(new OpenApiSecurityRequirement { { bearerScheme, [] } });
        });

        return services;
    }
}
