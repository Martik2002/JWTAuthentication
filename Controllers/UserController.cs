using JWTAuthentication.Interfaces;
using JWTAuthentication.Services;
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

    [HttpPost]
    public async Task<IActionResult> Register(string username, string email, string password)
    {
        var result = await _userService.Register(username, email, password);
        return Ok(result);
    }
}