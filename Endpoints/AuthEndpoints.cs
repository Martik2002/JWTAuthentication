using Carter;
using JWTAuthentication.Application.Abstractions.Interfaces;
using JWTAuthentication.Application.ActiveUser;
using JWTAuthentication.Application.Login;
using JWTAuthentication.Application.Register.CreateCommand;
using JWTAuthentication.Application.SignOut;
using JWTAuthentication.Application.ValidateRefreshToken;
using JWTAuthentication.Common.Constants;
using JWTAuthentication.Common.Models.AuthResponse;
using JWTAuthentication.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;


namespace JWTAuthentication.Endpoints;

public class AuthEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("app/auth");

        group.MapPost("login", Login).WithName(nameof(Login));

        group.MapPost("validateRefreshToken", ValidateRefreshToken)
            .RequireAuthorization()
            .WithName(nameof(ValidateRefreshToken));

        group.MapPost("register", Register).WithName(nameof(Register));

        group.MapPost("signOut", SignOut)
            .RequireAuthorization()
            .WithName(nameof(SignOut));
        
        group.MapGet("GetActiveUser", GetActiveUser)
            .RequireAuthorization()
            .WithName(nameof(GetActiveUser));

        group.MapGet("TestLogin", TestLogin)
            .RequireAuthorization()
            .WithName(nameof(TestLogin));

        group.MapGet("TestAdmin", TestAdmin)
            .RequireAuthorization()
            .WithName(nameof(TestAdmin));
        
    }

    private async Task<Results<Ok<JwtAuthResult>, BadRequest>> Login(LoginCommand command,
        IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(command, cancellationToken);
        if (result == null)
        {
            return TypedResults.BadRequest();
        }

        return TypedResults.Ok(result);
    }


    private async Task<Results<Ok<User>, UnauthorizedHttpResult>> ValidateRefreshToken(
        ValidateRefreshTokenCommand command, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(command, cancellationToken);
        if (result is null)
        {
            return TypedResults.Unauthorized();
        }

        return TypedResults.Ok(result);
    }

    private async Task<Results<Ok<string>, BadRequest>> Register(UserRegisterCommand command,
        IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(command, cancellationToken);
        if (result == null)
        {
            return TypedResults.BadRequest();
        }

        return TypedResults.Ok(result);
    }

    private async Task<Results<Ok<string>, BadRequest>> SignOut(SignOutCommand command, IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(command, cancellationToken);
        if (result == null)
        {
            return TypedResults.BadRequest();
        }

        return TypedResults.Ok(result);
    }

    private async Task<Results<Ok<ActiveUserResponse>, BadRequest>> GetActiveUser([AsParameters]GetActiveUserCommand command,IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(command, cancellationToken);
        if (result == null)
        {
            return TypedResults.BadRequest();
        }

        return TypedResults.Ok(result);
    }

    [Authorize(Roles = Role.User)]
    public Task<IResult> TestLogin()
    {
        return Task.FromResult<IResult>(TypedResults.Ok("You are logged in"));
    }

    [Authorize(Roles = Role.Admin)]
    public Task<IResult> TestAdmin()
    {
        return Task.FromResult<IResult>(TypedResults.Ok("You are admin"));
    }
}