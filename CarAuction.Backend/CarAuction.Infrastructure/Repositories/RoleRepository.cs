using CarAuction.Application.Interfaces.Repositories;
using CarAuction.Domain.Entities;
using CarAuction.Infrastructure.Data.Connection;
using Npgsql;
using NpgsqlTypes;

namespace CarAuction.Infrastructure.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public RoleRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<Role>> GetAllAsync()
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            "SELECT id, name FROM roles ORDER BY id ASC", connection);

        var roles = new List<Role>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            roles.Add(new Role
            {
                Id = reader.GetFieldValue<int>(reader.GetOrdinal("id")),
                Name = reader.GetFieldValue<string>(reader.GetOrdinal("name"))
            });
        }

        return roles;
    }

    public async Task<Role?> GetByIdAsync(int id)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            "SELECT id, name FROM roles WHERE id = @id", connection);

        command.Parameters.Add(new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = id });

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Role
            {
                Id = reader.GetFieldValue<int>(reader.GetOrdinal("id")),
                Name = reader.GetFieldValue<string>(reader.GetOrdinal("name"))
            };
        }

        return null;
    }

    public async Task<bool> NameExistsAsync(string name)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            "SELECT COUNT(1) FROM roles WHERE name = @name", connection);

        command.Parameters.Add(new NpgsqlParameter("@name", NpgsqlDbType.Varchar) { Value = name });

        var count = (long)(await command.ExecuteScalarAsync())!;
        return count > 0;
    }

    public async Task<int> CreateAsync(Role role)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            "INSERT INTO roles (name) VALUES (@name) RETURNING id", connection);

        command.Parameters.Add(new NpgsqlParameter("@name", NpgsqlDbType.Varchar) { Value = role.Name });

        var id = (int)(await command.ExecuteScalarAsync())!;
        return id;
    }

    public async Task DeleteAsync(int id)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            "DELETE FROM roles WHERE id = @id", connection);

        command.Parameters.Add(new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = id });

        await command.ExecuteNonQueryAsync();
    }

    public async Task<bool> IsAssignedToUsersAsync(int roleId)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            "SELECT COUNT(1) FROM users WHERE role_id = @roleId", connection);

        command.Parameters.Add(new NpgsqlParameter("@roleId", NpgsqlDbType.Integer) { Value = roleId });

        var count = (long)(await command.ExecuteScalarAsync())!;
        return count > 0;
    }

    public async Task<List<Permission>> GetAllPermissionsAsync()
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            "SELECT id, name, description FROM permissions ORDER BY id ASC", connection);

        var permissions = new List<Permission>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            permissions.Add(new Permission
            {
                Id = reader.GetFieldValue<int>(reader.GetOrdinal("id")),
                Name = reader.GetFieldValue<string>(reader.GetOrdinal("name")),
                Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetFieldValue<string>(reader.GetOrdinal("description"))
            });
        }

        return permissions;
    }

    public async Task<List<Permission>> GetRolePermissionsAsync(int roleId)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            @"SELECT p.id, p.name, p.description 
              FROM permissions p 
              INNER JOIN role_permissions rp ON p.id = rp.permission_id 
              WHERE rp.role_id = @roleId", connection);

        command.Parameters.Add(new NpgsqlParameter("@roleId", NpgsqlDbType.Integer) { Value = roleId });

        var permissions = new List<Permission>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            permissions.Add(new Permission
            {
                Id = reader.GetFieldValue<int>(reader.GetOrdinal("id")),
                Name = reader.GetFieldValue<string>(reader.GetOrdinal("name")),
                Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetFieldValue<string>(reader.GetOrdinal("description"))
            });
        }

        return permissions;
    }

    public async Task<bool> PermissionExistsAsync(int permissionId)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            "SELECT 1 FROM permissions WHERE id = @id LIMIT 1", connection);

        command.Parameters.Add(new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = permissionId });

        var result = await command.ExecuteScalarAsync();
        return result != null && result != DBNull.Value;
    }

    public async Task AssignPermissionAsync(int roleId, int permissionId)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            "INSERT INTO role_permissions (role_id, permission_id) VALUES (@roleId, @permissionId) ON CONFLICT DO NOTHING", connection);

        command.Parameters.Add(new NpgsqlParameter("@roleId", NpgsqlDbType.Integer) { Value = roleId });
        command.Parameters.Add(new NpgsqlParameter("@permissionId", NpgsqlDbType.Integer) { Value = permissionId });

        await command.ExecuteNonQueryAsync();
    }

    public async Task RemovePermissionAsync(int roleId, int permissionId)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            "DELETE FROM role_permissions WHERE role_id = @roleId AND permission_id = @permissionId", connection);

        command.Parameters.Add(new NpgsqlParameter("@roleId", NpgsqlDbType.Integer) { Value = roleId });
        command.Parameters.Add(new NpgsqlParameter("@permissionId", NpgsqlDbType.Integer) { Value = permissionId });

        await command.ExecuteNonQueryAsync();
    }
}
