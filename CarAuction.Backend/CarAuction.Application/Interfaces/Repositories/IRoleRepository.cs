using CarAuction.Domain.Entities;

namespace CarAuction.Application.Interfaces.Repositories;

public interface IRoleRepository
{
    Task<List<Role>> GetAllAsync();
    Task<Role?> GetByIdAsync(int id);
    Task<bool> NameExistsAsync(string name);
    Task<int> CreateAsync(Role role);
    Task DeleteAsync(int id);
    Task<bool> IsAssignedToUsersAsync(int roleId);
    Task<List<Permission>> GetAllPermissionsAsync();
    Task<List<Permission>> GetRolePermissionsAsync(int roleId);
    Task<bool> PermissionExistsAsync(int permissionId);
    Task AssignPermissionAsync(int roleId, int permissionId);
    Task RemovePermissionAsync(int roleId, int permissionId);
}
