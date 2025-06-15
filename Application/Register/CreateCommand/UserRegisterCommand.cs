using JWTAuthentication.Application.Abstractions.Interfaces;

namespace JWTAuthentication.Application.Register.CreateCommand;

public struct UserRegisterCommand : IRequest<string>
{
    public string Id { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string MiddleName { get; init; }
    public string PhoneNumber { get; init; }
    public DateTime DateOfBirth { get; init; }
    public string UserName { get; init; }
    public string Password { get; init; }
    public string Email { get; init; }
    public string Role { get; init; }
    
}