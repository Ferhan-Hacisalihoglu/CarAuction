using CarAuction.Application.DTOs.Roles;
using CarAuction.Application.Interfaces.Repositories;
using CarAuction.Application.Interfaces.Services;

namespace CarAuction.Application.Services;

public class RolesPermissionsService : IRolesPermissionsService
{
    private readonly IRoleRepository _roleRepository;

    public RolesPermissionsService(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<List<RoleResponse>> GetAllRolesAsync()
    {
        var roles = await _roleRepository.GetAllAsync();
        return roles.Select(r => new RoleResponse(r.Id, r.Name)).ToList();
    }

    public async Task<RoleResponse> CreateRoleAsync(CreateRoleRequest request)
    {
        // Check if role name already exists
        if (await _roleRepository.NameExistsAsync(request.Name))
        {
            throw new InvalidOperationException($"Role '{request.Name}' already exists");
        }

        var role = new Domain.Entities.Role { Name = request.Name };
        var id = await _roleRepository.CreateAsync(role);

        return new RoleResponse(id, request.Name);
    }

    public async Task DeleteRoleAsync(int id)
    {
        // Check if role exists
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null)
        {
            throw new KeyNotFoundException($"Role with ID {id} not found");
        }

        // Check if role is assigned to any users
        if (await _roleRepository.IsAssignedToUsersAsync(id))
        {
            throw new InvalidOperationException($"Role '{role.Name}' is assigned to users and cannot be deleted");
        }

        await _roleRepository.DeleteAsync(id);
    }

    public async Task<List<PermissionResponse>> GetAllPermissionsAsync()
    {
        var permissions = await _roleRepository.GetAllPermissionsAsync();
        return permissions.Select(p => new PermissionResponse(p.Id, p.Name, p.Description)).ToList();
    }

    public async Task<List<PermissionResponse>> GetRolePermissionsAsync(int roleId)
    {
        // Check if role exists
        var role = await _roleRepository.GetByIdAsync(roleId);
        if (role == null)
        {
            throw new KeyNotFoundException($"Role with ID {roleId} not found");
        }

        var permissions = await _roleRepository.GetRolePermissionsAsync(roleId);
        return permissions.Select(p => new PermissionResponse(p.Id, p.Name, p.Description)).ToList();
    }

    public async Task AssignPermissionAsync(int roleId, AssignPermissionRequest request)
    {
        // Check if role exists
        var role = await _roleRepository.GetByIdAsync(roleId);
        if (role == null)
        {
            throw new KeyNotFoundException($"Role with ID {roleId} not found");
        }

        // Check if permission exists
        if (!await _roleRepository.PermissionExistsAsync(request.PermissionId))
        {
            throw new InvalidOperationException($"Permission with ID {request.PermissionId} does not exist");
        }

        await _roleRepository.AssignPermissionAsync(roleId, request.PermissionId);
    }

    public async Task RemovePermissionAsync(int roleId, int permissionId)
    {
        // Check if role exists
        var role = await _roleRepository.GetByIdAsync(roleId);
        if (role == null)
        {
            throw new KeyNotFoundException($"Role with ID {roleId} not found");
        }

        await _roleRepository.RemovePermissionAsync(roleId, permissionId);
    }
}
