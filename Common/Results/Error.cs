namespace JWTAuthentication.Common.Results;

public enum ErrorType
{
    Validation,
    Unauthorized,
    NotFound,
    Conflict
}

/// <summary>
/// An expected failure of a use case (wrong password, duplicate email, ...).
/// Expected failures are returned as values instead of thrown as exceptions,
/// so exceptions are reserved for truly unexpected problems.
/// </summary>
public sealed record Error(string Code, string Description, ErrorType Type)
{
    public static Error Validation(string code, string description) => new(code, description, ErrorType.Validation);
    public static Error Unauthorized(string code, string description) => new(code, description, ErrorType.Unauthorized);
    public static Error NotFound(string code, string description) => new(code, description, ErrorType.NotFound);
    public static Error Conflict(string code, string description) => new(code, description, ErrorType.Conflict);
}
