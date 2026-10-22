using FluentValidation;
using JWTAuthentication.Application.Abstractions.Interfaces;
using JWTAuthentication.Common.Results;

namespace JWTAuthentication.Application.Auth.Register;

// Note: there is intentionally no "Role" field. New users always get the User role;
// letting the client choose its role would let anyone register as Admin.
public sealed record RegisterCommand(
    string UserName,
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string MiddleName,
    string PhoneNumber,
    DateTime? DateOfBirth) : IRequest<Result<RegisterResponse>>;

public sealed record RegisterResponse(string UserId);

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.UserName)
            .NotEmpty()
            .Length(3, 50)
            .Matches("^[a-zA-Z0-9._-]+$")
            .WithMessage("Username can contain only letters, digits, '.', '_' and '-'.");

        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);

        // Complexity rules (upper/lower case, digit, symbol) are enforced by ASP.NET Core Identity
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(128);

        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MiddleName).MaximumLength(100);
        RuleFor(x => x.PhoneNumber).MaximumLength(20);

        RuleFor(x => x.DateOfBirth)
            .NotNull()
            .Must(date => date is null || date.Value.Date < timeProvider.GetUtcNow().UtcDateTime.Date)
            .WithMessage("Date of birth must be in the past.");
    }
}
