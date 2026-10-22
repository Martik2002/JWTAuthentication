namespace JWTAuthentication.Common.Results;

public static class ResultExtensions
{
    /// <summary>
    /// Converts an expected failure into an RFC 7807 ProblemDetails response.
    /// </summary>
    public static IResult ToProblem(this Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        return Microsoft.AspNetCore.Http.Results.Problem(
            statusCode: statusCode,
            title: error.Code,
            detail: error.Description);
    }
}
