using JWTAuthentication.Common.Models;
using JWTAuthentication.Entities;
using JWTAuthentication.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;

namespace JWTAuthentication.Common.Services;

public sealed class JwtTokenService(IOptions<JwtSettings> options) : IJwtTokenService
{
    private readonly JwtSettings _jwtSetting = options.Value;

    public string GenerateToken(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrEmpty(user.UserName, nameof(user.UserName));
        ArgumentException.ThrowIfNullOrEmpty(user.Email, nameof(user.Email));

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email),
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSetting.Key));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _jwtSetting.Issuer,
            audience: _jwtSetting.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSetting.AccessTokenExpiration),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
}