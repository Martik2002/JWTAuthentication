using JWTAuthentication.Application.Abstractions.Interfaces;

namespace JWTAuthentication.Application.Register;

public struct UserRegisterCommand : IRequest<string>
{
    public string Email { get; init; }
}