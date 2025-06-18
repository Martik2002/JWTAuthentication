using System.Security.Claims;
using JWTAuthentication.Application.Abstractions.Interfaces;

namespace JWTAuthentication.Application.Abstractions.Services;

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : IUser
{
    public string Id => httpContextAccessor.HttpContext?.User?.Claims
        .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
}