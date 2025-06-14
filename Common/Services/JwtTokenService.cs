﻿using JWTAuthentication.Common.Models;
using JWTAuthentication.Entities;
using JWTAuthentication.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using JWTAuthentication.Common.Models.AuthResponse;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace JWTAuthentication.Common.Services;

public sealed class JwtTokenService(UserManager<User> userManager, IOptions<JwtSettings> options)
    : IJwtTokenService
{
    private readonly JwtSettings _jwtSetting = options.Value;

    public async Task<JwtAuthResult> GenerateToken(User user)
    {
        var accessToken = await GenerateAcseccToken(user);

        var refreshToken = new RefreshToken
        {
            UserName = user.UserName,
            TokenString = GenerateRefreshTokenString(),
            Expires = DateTime.UtcNow.AddDays(_jwtSetting.RefreshTokenExpiration),
        };

        return new JwtAuthResult
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
        };
    }
    private async Task<string> GenerateAcseccToken(User user)
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
        var roles = await userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

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
    
    private static string GenerateRefreshTokenString()
    {
        var randomNumber = new byte[32];
        using var randomNumberGenerator = RandomNumberGenerator.Create();
        randomNumberGenerator.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}