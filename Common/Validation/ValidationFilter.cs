using FluentValidation;

namespace JWTAuthentication.Common.Validation;

/// <summary>
/// Endpoint filter that validates the request body with FluentValidation
/// and returns a 400 ValidationProblem before the handler runs.
/// </summary>
public sealed class ValidationFilter<TRequest>(IValidator<TRequest> validator) : IEndpointFilter
{
    public async ValueTask<object> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<TRequest>().FirstOrDefault();
        if (request is null)
        {
            return Microsoft.AspNetCore.Http.Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Request body is required.");
        }

        var result = await validator.ValidateAsync(request, context.HttpContext.RequestAborted);
        if (result.IsValid)
        {
            return await next(context);
        }

        var errors = result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        return Microsoft.AspNetCore.Http.Results.ValidationProblem(errors);
    }
}

public static class ValidationFilterExtensions
{
    public static RouteHandlerBuilder WithValidation<TRequest>(this RouteHandlerBuilder builder) =>
        builder
            .AddEndpointFilter<ValidationFilter<TRequest>>()
            .ProducesValidationProblem();
}
