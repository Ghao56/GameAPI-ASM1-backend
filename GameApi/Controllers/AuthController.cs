using GameApi.DTOs;
using GameApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var account = await _authService.RegisterAsync(request);
        if (account is null)
        {
            return Conflict(new { message = "Tên đăng nhập đã tồn tại" });
        }

        return StatusCode(StatusCodes.Status201Created, account);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        if (response is null)
        {
            return Unauthorized(new { message = "Sai tên đăng nhập hoặc mật khẩu" });
        }

        return Ok(response);
    }
}
