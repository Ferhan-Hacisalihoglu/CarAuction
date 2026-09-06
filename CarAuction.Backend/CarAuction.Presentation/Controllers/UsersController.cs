using CarAuction.Application.DTOs;
using CarAuction.Application.DTOs.Users;
using CarAuction.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarAuction.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<UserListResponse>>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] string? search = null)
    {
        var result = await _userService.GetAllAsync(page, limit, search);
        return Ok(ApiResponse<PaginatedResponse<UserListResponse>>.SuccessResponse(result));
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserListResponse>>> GetById(int id)
    {
        var result = await _userService.GetByIdAsync(id);
        if (result == null)
        {
            return NotFound(ApiResponse<UserListResponse>.ErrorResponse("User not found"));
        }
        return Ok(ApiResponse<UserListResponse>.SuccessResponse(result));
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserListResponse>>> UpdateProfile(
        [FromBody] UpdateProfileRequest request)
    {
        var userId = GetUserId();
        var result = await _userService.UpdateProfileAsync(userId, request);
        return Ok(ApiResponse<UserListResponse>.SuccessResponse(result, "Profile updated successfully"));
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<UserListResponse>>> UpdateStatus(
        int id, [FromBody] UpdateUserStatusRequest request)
    {
        var result = await _userService.UpdateStatusAsync(id, request);
        return Ok(ApiResponse<UserListResponse>.SuccessResponse(result, "Status updated successfully"));
    }

    [HttpPut("{id:int}/role")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<UserListResponse>>> UpdateRole(
        int id, [FromBody] UpdateUserRoleRequest request)
    {
        var result = await _userService.UpdateRoleAsync(id, request);
        return Ok(ApiResponse<UserListResponse>.SuccessResponse(result, "Role updated successfully"));
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = GetUserId();
        await _userService.ChangePasswordAsync(userId, request);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Password changed successfully"));
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
