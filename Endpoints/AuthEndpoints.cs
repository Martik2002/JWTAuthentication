using Carter;
using JWTAuthentication.Application.Abstractions.Interfaces;
using JWTAuthentication.Application.Abstractions.Mediator;
using JWTAuthentication.Application.Login;
using JWTAuthentication.Application.Register;
using JWTAuthentication.Application.Register.CreateCommand;
using JWTAuthentication.Application.ValidateRefreshToken;
using JWTAuthentication.Common.Constants;
using JWTAuthentication.Common.Models.AuthResponse;
using JWTAuthentication.Entities;
using JWTAuthentication.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;


namespace JWTAuthentication.Endpoints;

public class AuthEndpoints : ICarterModule
{
    public  void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("app/auth");
        
        group.MapPost("login", Login).WithName(nameof(Login));
        
        group.MapPost("Validate-Token", ValidateRefreshToken)
            .RequireAuthorization()
            .WithName(nameof(ValidateRefreshToken));
        
        group.MapPost("register", Register).WithName(nameof(Register));
        
        group.MapGet("TestLogin", TestLogin)
            .RequireAuthorization()
            .WithName(nameof(TestLogin));
        
        group.MapGet("TestAdmin", TestAdmin)
            .RequireAuthorization()
            .WithName(nameof(TestAdmin));
        
        group.MapPost("testMediator", TestMediator)
            .WithName(nameof(TestMediator));
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
        IMediator mediator,
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
    public async Task<IResult> TestLogin()
    {
        return TypedResults.Ok("You are logged in");
    }

    [Authorize(Roles = Role.Admin)]
    public async Task<IResult> TestAdmin()
    {
        return TypedResults.Ok("You are admin");
    }

    public async Task<Results<Ok<string>, BadRequest>> TestMediator(
        [FromBody] UserRegisterCommand command,
        [FromServices] Mediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(command, cancellationToken);
        if (result == null)
        {
            return TypedResults.BadRequest();
        }
        return TypedResults.Ok(result);
    }
}