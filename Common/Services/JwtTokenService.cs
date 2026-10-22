using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JWTAuthentication.Application.Auth;
using JWTAuthentication.Common.Models;
using JWTAuthentication.Common.Security;
using JWTAuthentication.Entities;
using JWTAuthentication.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace JWTAuthentication.Common.Services;

public sealed class JwtTokenService(
    UserManager<User> userManager,
    IOptions<JwtSettings> options,
    TimeProvider timeProvider) : IJwtTokenService
{
    private readonly JwtSettings _settings = options.Value;

    public async Task<AuthResponse> IssueTokensAsync(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var accessTokenExpiresAt = now.AddMinutes(_settings.AccessTokenExpiration);
        var refreshTokenExpiresAt = now.AddDays(_settings.RefreshTokenExpiration);

        var accessToken = await CreateAccessTokenAsync(user, now, accessTokenExpiresAt);
        var refreshToken = RefreshTokenHasher.Generate();

        // Only the hash is stored. Issuing a new refresh token replaces the old one (rotation),
        // so every refresh token can be used exactly once.
        user.RefreshToken = RefreshTokenHasher.Hash(refreshToken);
        user.RefreshTokenExpiry = refreshTokenExpiresAt;

        return new AuthResponse(accessToken, accessTokenExpiresAt, refreshToken, refreshTokenExpiresAt);
    }

    private async Task<string> CreateAccessTokenAsync(User user, DateTime now, DateTime expiresAt)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
        };

        var roles = await userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
