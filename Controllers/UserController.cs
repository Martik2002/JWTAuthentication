using JWTAuthentication.Common.Constants;
using JWTAuthentication.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JWTAuthentication.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(string username, string password)
    {
        var result = await _userService.LoginAsync(username, password);
        if (result == null)
        {
            return BadRequest();
        }
        return Ok(result);
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register(string username, string email, string password)
    {
        var result = await _userService.Register(username, email, password);
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