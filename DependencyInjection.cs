using System.Text;
using JWTAuthentication.Database;
using JWTAuthentication.Interfaces;
using JWTAuthentication.Services;
using JWTAuthentication.Common.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace JWTAuthentication;

public static class DependencyInjection
{
    public static IServiceCollection ServiceProvider(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllersWithViews();
        services.AddEndpointsApiExplorer();
        services.AddTransient<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());
        services.AddTransient<IUserService, UsersService>();
        services.AddTransient<IPasswordHasher, PasswordHasher>();
        services.AddTransient<IJwtTokenService, JwtTokenService>();
        services.AddControllers();

        #region Database Configuration
            services.AddDbContext<ApplicationDbContext>((options) =>
            {
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            });
            services.AddHealthChecks()
                .AddDbContextCheck<ApplicationDbContext>();
        #endregion
        
        #region Swagger Configuration
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "JWT Authorization API", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Id = "Bearer",
                                Type = ReferenceType.SecurityScheme
                            }
                        },
                        []
                    }
                });
            });
        #endregion

        #region Authentication Configuration
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = configuration["JwtSettings:Issuer"],
                        ValidateAudience = true,
                        ValidAudience = configuration["JwtSettings:Audience"],
                        ValidateLifetime = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"]!)),
                        ValidateIssuerSigningKey = true
                    };
                });
        #endregion

        return services;
    }
}