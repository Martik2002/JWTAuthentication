using FluentValidation;
using JWTAuthentication.Application.Abstractions.Interfaces;
using JWTAuthentication.Common.Results;

namespace JWTAuthentication.Application.Auth.Login;

public sealed record LoginCommand(string UserName, string Password) : IRequest<Result<AuthResponse>>;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(128);
    }
}
