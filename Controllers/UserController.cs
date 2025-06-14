using JWTAuthentication.Common.Constants;
using JWTAuthentication.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JWTAuthentication.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(string username, string password, CancellationToken cancellationToken)
    {
        var result = await userService.LoginAsync(username, password, cancellationToken);
        if (result == null)
        {
            return BadRequest();
        }
        return Ok(result);
    }

    [HttpPost("Validate-Token")]
    public async Task<IActionResult> ValidateRefreshToken(string userId, string password, CancellationToken cancellationToken)
    {
        var result = await userService.ValidateRefreshTokenAsync(userId, password, cancellationToken);
        if (result is null)
        {
            return Unauthorized("Invalid refresh token");
        }
        return Ok(result);
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register(string username, string email, string password)
    {
        var result = await userService.Register(username, email, password);
        return Ok(result);
    }

    [Authorize(Roles = Role.User)]
    [HttpGet("TestLogin")]
    public async Task<IActionResult> TestLogin()
    {
        return Ok("You are logged in");
    }

    [Authorize(Roles = Role.Admin)]
    [HttpGet("TestAdmin")]
    public async Task<IActionResult> TestAdmin()
    {
        return Ok("You are admin");
    }
}