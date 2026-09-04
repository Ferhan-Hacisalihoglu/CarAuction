using CarAuction.Application.DTOs;
using CarAuction.Application.DTOs.Roles;
using CarAuction.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarAuction.Presentation.Controllers;

[ApiController]
[Route("api")]
[Authorize(Roles = "Admin")]
public class RolesPermissionsController : ControllerBase
{
    private readonly IRolesPermissionsService _service;

    public RolesPermissionsController(IRolesPermissionsService service)
    {
        _service = service;
    }

    // Roles
    [HttpGet("roles")]
    public async Task<ActionResult<ApiResponse<List<RoleResponse>>>> GetRoles()
    {
        var result = await _service.GetAllRolesAsync();
        return Ok(ApiResponse<List<RoleResponse>>.SuccessResponse(result));
    }

    [HttpPost("roles")]
    public async Task<ActionResult<ApiResponse<RoleResponse>>> CreateRole([FromBody] CreateRoleRequest request)
    {
        var result = await _service.CreateRoleAsync(request);
        return CreatedAtAction(nameof(GetRoles), ApiResponse<RoleResponse>.SuccessResponse(result, "Role created successfully"));
    }

    [HttpDelete("roles/{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteRole(int id)
    {
        await _service.DeleteRoleAsync(id);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Role deleted successfully"));
    }

    // Permissions
    [HttpGet("permissions")]
    public async Task<ActionResult<ApiResponse<List<PermissionResponse>>>> GetPermissions()
    {
        var result = await _service.GetAllPermissionsAsync();
        return Ok(ApiResponse<List<PermissionResponse>>.SuccessResponse(result));
    }

    [HttpGet("roles/{roleId:int}/permissions")]
    public async Task<ActionResult<ApiResponse<List<PermissionResponse>>>> GetRolePermissions(int roleId)
    {
        var result = await _service.GetRolePermissionsAsync(roleId);
        return Ok(ApiResponse<List<PermissionResponse>>.SuccessResponse(result));
    }

    [HttpPost("roles/{roleId:int}/permissions")]
    public async Task<ActionResult<ApiResponse<object>>> AssignPermission(
        int roleId, [FromBody] AssignPermissionRequest request)
    {
        await _service.AssignPermissionAsync(roleId, request);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Permission assigned successfully"));
    }

    [HttpDelete("roles/{roleId:int}/permissions/{permissionId:int}")]
    public async Task<ActionResult<ApiResponse<object>>> RemovePermission(int roleId, int permissionId)
    {
        await _service.RemovePermissionAsync(roleId, permissionId);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Permission removed successfully"));
    }
}
