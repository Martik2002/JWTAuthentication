using JWTAuthentication.Application.Abstractions.Interfaces;
using JWTAuthentication.Entities;

namespace JWTAuthentication.Application.ValidateRefreshToken;

public abstract class ValidateRefreshTokenCommand : IRequest<User>
{
    public string UserId { get; set; }  
    public string RefreshToken { get; set; }
}