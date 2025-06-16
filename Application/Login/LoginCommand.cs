using JWTAuthentication.Application.Abstractions.Interfaces;
using JWTAuthentication.Common.Models.AuthResponse;

namespace JWTAuthentication.Application.Login;

public  sealed class LoginCommand : IRequest<JwtAuthResult>
{
    public string Username { get; set; }
    public string Password { get; set; }
}