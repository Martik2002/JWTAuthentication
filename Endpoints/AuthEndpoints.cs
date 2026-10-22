using JWTAuthentication.Application.Abstractions.Interfaces;
using JWTAuthentication.Application.Auth;
using JWTAuthentication.Application.Auth.Login;
using JWTAuthentication.Application.Auth.Logout;
using JWTAuthentication.Application.Auth.Me;
using JWTAuthentication.Application.Auth.Refresh;
using JWTAuthentication.Application.Auth.Register;
using JWTAuthentication.Common.Constants;
using JWTAuthentication.Common.Results;
using JWTAuthentication.Common.Validation;
using JWTAuthentication.Infrastructure;

namespace JWTAuthentication.Endpoints;

public sealed class AuthEndpoints : EndpointGroupBase
{
    public const string RateLimitPolicy = "auth";

    public override void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth")
            .WithOpenApi();

        group.MapPost("register", Register)
            .WithName(nameof(Register))
            .WithValidation<RegisterCommand>()
            .RequireRateLimiting(RateLimitPolicy)
            .Produces<RegisterResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("login", Login)
            .WithName(nameof(Login))
            .WithValidation<LoginCommand>()
            .RequireRateLimiting(RateLimitPolicy)
            .Produces<AuthResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        // Anonymous on purpose: the client calls this when its access token has expired.
        group.MapPost("refresh", Refresh)
            .WithName(nameof(Refresh))
            .WithValidation<RefreshTokenCommand>()
            .RequireRateLimiting(RateLimitPolicy)
            .Produces<AuthResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("logout", Logout)
            .WithName(nameof(Logout))
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent);

        group.MapGet("me", Me)
            .WithName(nameof(Me))
            .RequireAuthorization()
            .Produces<CurrentUserResponse>();

        group.MapGet("admin", Admin)
            .WithName(nameof(Admin))
            .RequireAuthorization(policy => policy.RequireRole(Role.Admin))
            .Produces<string>();
    }

    private static async Task<IResult> Register(RegisterCommand command, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(command, cancellationToken);
        return result.IsSuccess
            ? TypedResults.Created("/api/auth/me", result.Value)
            : result.Error.ToProblem();
    }

    private static async Task<IResult> Login(LoginCommand command, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(command, cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.Error.ToProblem();
    }

    private static async Task<IResult> Refresh(RefreshTokenCommand command, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(command, cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.Error.ToProblem();
    }

    private static async Task<IResult> Logout(IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(new LogoutCommand(), cancellationToken);
        return result.IsSuccess ? TypedResults.NoContent() : result.Error.ToProblem();
    }

    private static async Task<IResult> Me(IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(new GetCurrentUserQuery(), cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.Error.ToProblem();
    }

    private static IResult Admin() => TypedResults.Ok("You are an admin.");
}
