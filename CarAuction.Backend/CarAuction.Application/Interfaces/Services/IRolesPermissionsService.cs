using CarAuction.Application.DTOs.Roles;

namespace CarAuction.Application.Interfaces.Services;

public interface IRolesPermissionsService
{
    Task<List<RoleResponse>> GetAllRolesAsync();
    Task<RoleResponse> CreateRoleAsync(CreateRoleRequest request);
    Task DeleteRoleAsync(int id);
    Task<List<PermissionResponse>> GetAllPermissionsAsync();
    Task<List<PermissionResponse>> GetRolePermissionsAsync(int roleId);
    Task AssignPermissionAsync(int roleId, AssignPermissionRequest request);
    Task RemovePermissionAsync(int roleId, int permissionId);
}
