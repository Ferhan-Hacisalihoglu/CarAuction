using CarAuction.Application.DTOs.Bids;
using CarAuction.Application.Interfaces.Repositories;
using CarAuction.Domain.Entities;
using CarAuction.Infrastructure.Data.Connection;
using Npgsql;
using NpgsqlTypes;

namespace CarAuction.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public UserRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            @"SELECT u.id, u.first_name, u.last_name, u.email, u.password_hash, u.salt, 
                     u.role_id, u.refresh_token, u.refresh_token_expiry, u.is_active, u.created_at,
                     r.name as role_name
              FROM users u 
              LEFT JOIN roles r ON u.role_id = r.id 
              WHERE u.id = @id", connection);

        command.Parameters.Add(new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = id });

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapUser(reader);
        }

        return null;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            @"SELECT u.id, u.first_name, u.last_name, u.email, u.password_hash, u.salt, 
                     u.role_id, u.refresh_token, u.refresh_token_expiry, u.is_active, u.created_at,
                     r.name as role_name
              FROM users u 
              LEFT JOIN roles r ON u.role_id = r.id 
              WHERE u.email = @email", connection);

        command.Parameters.Add(new NpgsqlParameter("@email", NpgsqlDbType.Varchar) { Value = email });

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapUser(reader);
        }

        return null;
    }

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            @"SELECT u.id, u.first_name, u.last_name, u.email, u.password_hash, u.salt, 
                     u.role_id, u.refresh_token, u.refresh_token_expiry, u.is_active, u.created_at,
                     r.name as role_name
              FROM users u 
              LEFT JOIN roles r ON u.role_id = r.id 
              WHERE u.refresh_token = @refreshToken AND u.is_active = TRUE", connection);

        command.Parameters.Add(new NpgsqlParameter("@refreshToken", NpgsqlDbType.Varchar) { Value = refreshToken });

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapUser(reader);
        }

        return null;
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            "SELECT COUNT(1) FROM users WHERE email = @email", connection);

        command.Parameters.Add(new NpgsqlParameter("@email", NpgsqlDbType.Varchar) { Value = email });

        var count = (long)(await command.ExecuteScalarAsync())!;
        return count > 0;
    }

    public async Task<int> CreateAsync(User user)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            @"INSERT INTO users (first_name, last_name, email, password_hash, salt, role_id, is_active, created_at) 
              VALUES (@firstName, @lastName, @email, @passwordHash, @salt, @roleId, @isActive, @createdAt) 
              RETURNING id", connection);

        command.Parameters.Add(new NpgsqlParameter("@firstName", NpgsqlDbType.Varchar) { Value = user.FirstName });
        command.Parameters.Add(new NpgsqlParameter("@lastName", NpgsqlDbType.Varchar) { Value = user.LastName });
        command.Parameters.Add(new NpgsqlParameter("@email", NpgsqlDbType.Varchar) { Value = user.Email });
        command.Parameters.Add(new NpgsqlParameter("@passwordHash", NpgsqlDbType.Varchar) { Value = user.PasswordHash });
        command.Parameters.Add(new NpgsqlParameter("@salt", NpgsqlDbType.Varchar) { Value = user.Salt });
        command.Parameters.Add(new NpgsqlParameter("@roleId", NpgsqlDbType.Integer) { Value = (object?)user.RoleId ?? DBNull.Value });
        command.Parameters.Add(new NpgsqlParameter("@isActive", NpgsqlDbType.Boolean) { Value = user.IsActive });
        command.Parameters.Add(new NpgsqlParameter("@createdAt", NpgsqlDbType.TimestampTz) { Value = user.CreatedAt });

        var id = (int)(await command.ExecuteScalarAsync())!;
        return id;
    }

    public async Task UpdateRefreshTokenAsync(int userId, string refreshToken, DateTime expiry)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            @"UPDATE users SET refresh_token = @refreshToken, refresh_token_expiry = @expiry WHERE id = @id", connection);

        command.Parameters.Add(new NpgsqlParameter("@refreshToken", NpgsqlDbType.Varchar) { Value = refreshToken });
        command.Parameters.Add(new NpgsqlParameter("@expiry", NpgsqlDbType.TimestampTz) { Value = expiry });
        command.Parameters.Add(new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = userId });

        await command.ExecuteNonQueryAsync();
    }

    public async Task ClearRefreshTokenAsync(int userId)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            @"UPDATE users SET refresh_token = NULL, refresh_token_expiry = NULL WHERE id = @id", connection);

        command.Parameters.Add(new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = userId });

        await command.ExecuteNonQueryAsync();
    }

    public async Task<int?> GetRoleIdByNameAsync(string roleName)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            "SELECT id FROM roles WHERE name = @name LIMIT 1", connection);

        command.Parameters.Add(new NpgsqlParameter("@name", NpgsqlDbType.Varchar) { Value = roleName });

        var result = await command.ExecuteScalarAsync();
        if (result == null || result == DBNull.Value)
        {
            return null;
        }

        return (int)result;
    }

    public async Task<(List<User> Users, int Total)> GetPaginatedAsync(int page, int limit, string? search = null)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        var offset = (page - 1) * limit;

        var searchCondition = string.IsNullOrEmpty(search) ? "" : 
            "WHERE (u.first_name ILIKE @search OR u.last_name ILIKE @search OR u.email ILIKE @search)";

        // Get total count
        await using var countCommand = new NpgsqlCommand($"SELECT COUNT(1) FROM users u {searchCondition}", connection);
        if (!string.IsNullOrEmpty(search))
        {
            countCommand.Parameters.Add(new NpgsqlParameter("@search", NpgsqlDbType.Varchar) { Value = $"%{search}%" });
        }
        var total = (long)(await countCommand.ExecuteScalarAsync())!;

        // Get paginated users
        await using var command = new NpgsqlCommand(
            @"SELECT u.id, u.first_name, u.last_name, u.email, u.password_hash, u.salt, 
                     u.role_id, u.refresh_token, u.refresh_token_expiry, u.is_active, u.created_at,
                     r.name as role_name
              FROM users u 
              LEFT JOIN roles r ON u.role_id = r.id 
              {searchCondition}
              ORDER BY u.created_at DESC 
              LIMIT @limit OFFSET @offset", connection);

        if (!string.IsNullOrEmpty(search))
        {
            command.Parameters.Add(new NpgsqlParameter("@search", NpgsqlDbType.Varchar) { Value = $"%{search}%" });
        }
        command.Parameters.Add(new NpgsqlParameter("@limit", NpgsqlDbType.Integer) { Value = limit });
        command.Parameters.Add(new NpgsqlParameter("@offset", NpgsqlDbType.Integer) { Value = offset });

        var users = new List<User>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            users.Add(MapUser(reader));
        }

        return (users, (int)total);
    }

    public async Task UpdateProfileAsync(int userId, string firstName, string lastName)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            @"UPDATE users SET first_name = @firstName, last_name = @lastName WHERE id = @id", connection);

        command.Parameters.Add(new NpgsqlParameter("@firstName", NpgsqlDbType.Varchar) { Value = firstName });
        command.Parameters.Add(new NpgsqlParameter("@lastName", NpgsqlDbType.Varchar) { Value = lastName });
        command.Parameters.Add(new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = userId });

        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateStatusAsync(int id, bool isActive)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            @"UPDATE users SET is_active = @isActive WHERE id = @id", connection);

        command.Parameters.Add(new NpgsqlParameter("@isActive", NpgsqlDbType.Boolean) { Value = isActive });
        command.Parameters.Add(new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = id });

        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateRoleAsync(int id, int roleId)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            @"UPDATE users SET role_id = @roleId WHERE id = @id", connection);

        command.Parameters.Add(new NpgsqlParameter("@roleId", NpgsqlDbType.Integer) { Value = roleId });
        command.Parameters.Add(new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = id });

        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdatePasswordAsync(int userId, string newHash, string newSalt)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            @"UPDATE users SET password_hash = @passwordHash, salt = @salt WHERE id = @id", connection);

        command.Parameters.Add(new NpgsqlParameter("@passwordHash", NpgsqlDbType.Varchar) { Value = newHash });
        command.Parameters.Add(new NpgsqlParameter("@salt", NpgsqlDbType.Varchar) { Value = newSalt });
        command.Parameters.Add(new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = userId });

        await command.ExecuteNonQueryAsync();
    }

    public async Task<bool> RoleExistsAsync(int roleId)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            "SELECT 1 FROM roles WHERE id = @id LIMIT 1", connection);

        command.Parameters.Add(new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = roleId });

        var result = await command.ExecuteScalarAsync();
        return result != null && result != DBNull.Value;
    }

    private static User MapUser(NpgsqlDataReader reader)
    {
        var roleId = reader.IsDBNull(reader.GetOrdinal("role_id")) ? null : reader.GetFieldValue<int?>(reader.GetOrdinal("role_id"));

        var user = new User
        {
            Id = reader.GetFieldValue<int>(reader.GetOrdinal("id")),
            FirstName = reader.GetFieldValue<string>(reader.GetOrdinal("first_name")),
            LastName = reader.GetFieldValue<string>(reader.GetOrdinal("last_name")),
            Email = reader.GetFieldValue<string>(reader.GetOrdinal("email")),
            PasswordHash = reader.GetFieldValue<string>(reader.GetOrdinal("password_hash")),
            Salt = reader.GetFieldValue<string>(reader.GetOrdinal("salt")),
            RoleId = roleId,
            RefreshToken = reader.IsDBNull(reader.GetOrdinal("refresh_token")) ? null : reader.GetFieldValue<string>(reader.GetOrdinal("refresh_token")),
            RefreshTokenExpiry = reader.IsDBNull(reader.GetOrdinal("refresh_token_expiry")) ? null : reader.GetFieldValue<DateTime?>(reader.GetOrdinal("refresh_token_expiry")),
            IsActive = reader.GetFieldValue<bool>(reader.GetOrdinal("is_active")),
            CreatedAt = reader.GetFieldValue<DateTime>(reader.GetOrdinal("created_at"))
        };

        if (!reader.IsDBNull(reader.GetOrdinal("role_name")))
        {
            user.Role = new Role
            {
                Id = roleId ?? 0,
                Name = reader.GetFieldValue<string>(reader.GetOrdinal("role_name"))
            };
        }

        return user;
    }

    public async Task<long> CountAsync()
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand("SELECT COUNT(1) FROM users", connection);
        return (long)(await command.ExecuteScalarAsync())!;
    }

    public async Task<long> CountRecentAsync(TimeSpan period)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            "SELECT COUNT(1) FROM users WHERE created_at > NOW() - @period", connection);
        command.Parameters.Add(new NpgsqlParameter("@period", NpgsqlDbType.Interval) { Value = period });
        return (long)(await command.ExecuteScalarAsync())!;
    }

    public async Task<List<User>> GetRecentAsync(int limit)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            @"SELECT id, first_name, last_name, email, password_hash, salt, 
                     role_id, refresh_token, refresh_token_expiry, is_active, created_at
              FROM users ORDER BY created_at DESC LIMIT @limit", connection);
        command.Parameters.Add(new NpgsqlParameter("@limit", NpgsqlDbType.Integer) { Value = limit });

        var users = new List<User>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var roleId = reader.IsDBNull(reader.GetOrdinal("role_id")) ? null : reader.GetFieldValue<int?>(reader.GetOrdinal("role_id"));
            users.Add(new User
            {
                Id = reader.GetFieldValue<int>(reader.GetOrdinal("id")),
                FirstName = reader.GetFieldValue<string>(reader.GetOrdinal("first_name")),
                LastName = reader.GetFieldValue<string>(reader.GetOrdinal("last_name")),
                Email = reader.GetFieldValue<string>(reader.GetOrdinal("email")),
                PasswordHash = reader.GetFieldValue<string>(reader.GetOrdinal("password_hash")),
                Salt = reader.GetFieldValue<string>(reader.GetOrdinal("salt")),
                RoleId = roleId,
                RefreshToken = reader.IsDBNull(reader.GetOrdinal("refresh_token")) ? null : reader.GetFieldValue<string>(reader.GetOrdinal("refresh_token")),
                RefreshTokenExpiry = reader.IsDBNull(reader.GetOrdinal("refresh_token_expiry")) ? null : reader.GetFieldValue<DateTime?>(reader.GetOrdinal("refresh_token_expiry")),
                IsActive = reader.GetFieldValue<bool>(reader.GetOrdinal("is_active")),
                CreatedAt = reader.GetFieldValue<DateTime>(reader.GetOrdinal("created_at"))
            });
        }
        return users;
    }
}
