using Carter;
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
        group.MapPost("Validate-Token", ValidateRefreshToken).WithName(nameof(ValidateRefreshToken));
        group.MapPost("register", Register).WithName(nameof(Register));
        group.MapGet("TestLogin", TestLogin).RequireAuthorization().WithName(nameof(TestLogin));
        group.MapPut("TestAdmin", TestAdmin).RequireAuthorization().WithName(nameof(TestAdmin));
    }

    private async Task<Results<Ok<JwtAuthResult>, BadRequest>> Login(string username, string password,
        IUserService userService, CancellationToken cancellationToken)
    {
        var result = await userService.LoginAsync(username, password, cancellationToken);
        if (result == null)
        {
            return TypedResults.BadRequest();
        }

        return TypedResults.Ok(result);
    }

    [HttpPost("Validate-Token")]
    private async Task<Results<Ok<User>, UnauthorizedHttpResult>> ValidateRefreshToken(string userId, string password,
        IUserService userService, CancellationToken cancellationToken)
    {
        var result = await userService.ValidateRefreshTokenAsync(userId, password, cancellationToken);
        if (result is null)
        {
            return TypedResults.Unauthorized();
        }

        return TypedResults.Ok(result);
    }

    [HttpPost("register")]
    private async Task<Results<Ok<string>, BadRequest>> Register(string username, string email,
        IUserService userService, string password)
    {
        var result = await userService.Register(username, email, password);
        if (result == null)
        {
            return TypedResults.BadRequest();
        }

        return TypedResults.Ok(result);
    }

    [Authorize(Roles = Role.User)]
    [HttpGet("TestLogin")]
    public async Task<IResult> TestLogin()
    {
        return TypedResults.Ok("You are logged in");
    }

    [Authorize(Roles = Role.Admin)]
    [HttpGet("TestAdmin")]
    public async Task<IResult> TestAdmin()
    {
        return TypedResults.Ok("You are admin");
    }
}