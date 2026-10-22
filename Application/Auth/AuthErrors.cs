using JWTAuthentication.Common.Results;

namespace JWTAuthentication.Application.Auth;

public static class AuthErrors
{
    // One generic message for every login failure (unknown user, wrong password, locked out),
    // so the API does not reveal which usernames exist.
    public static readonly Error InvalidCredentials =
        Error.Unauthorized("Auth.InvalidCredentials", "Invalid username or password.");

    public static readonly Error InvalidRefreshToken =
        Error.Unauthorized("Auth.InvalidRefreshToken", "The refresh token is invalid or has expired.");

    public static readonly Error UserNameTaken =
        Error.Conflict("Auth.UserNameTaken", "This username is already taken.");

    public static readonly Error EmailTaken =
        Error.Conflict("Auth.EmailTaken", "A user with this email already exists.");

    public static readonly Error UserNotFound =
        Error.NotFound("Auth.UserNotFound", "The user was not found.");
}
