using FluentValidation;
using JWTAuthentication.Application.Abstractions.Interfaces;
using JWTAuthentication.Common.Results;

namespace JWTAuthentication.Application.Auth.Refresh;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<Result<AuthResponse>>;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().MaximumLength(200);
    }
}
