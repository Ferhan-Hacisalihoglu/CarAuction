using CarAuction.Application.DTOs;
using CarAuction.Application.DTOs.Auth;
using CarAuction.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarAuction.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        return CreatedAtAction(nameof(Register), ApiResponse<AuthResponse>.SuccessResponse(result, "Registration successful"));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Login successful"));
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.Token);
        return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Token refreshed successfully"));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> Logout()
    {
        var userId = GetUserId();
        await _authService.LogoutAsync(userId);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Logout successful"));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserDto>>> Me()
    {
        var userId = GetUserId();
        var result = await _authService.GetCurrentUserAsync(userId);
        return Ok(ApiResponse<UserDto>.SuccessResponse(result!));
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user identity");
        }
        return userId;
    }
}
