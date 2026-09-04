using CarAuction.Application.DTOs.Groups;
using CarAuction.Application.Interfaces.Repositories;
using CarAuction.Domain.Entities;
using CarAuction.Infrastructure.Data.Connection;
using Npgsql;
using NpgsqlTypes;

namespace CarAuction.Infrastructure.Repositories;

public class GroupRepository : IGroupRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public GroupRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<GroupResponse>> GetAllAsync()
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            "SELECT id, name, description, created_by, created_at FROM groups ORDER BY created_at DESC", connection);

        var groups = new List<GroupResponse>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            groups.Add(new GroupResponse(
                reader.GetFieldValue<int>(reader.GetOrdinal("id")),
                reader.GetFieldValue<string>(reader.GetOrdinal("name")),
                reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetFieldValue<string>(reader.GetOrdinal("description")),
                reader.IsDBNull(reader.GetOrdinal("created_by")) ? 0 : reader.GetFieldValue<int>(reader.GetOrdinal("created_by")),
                reader.GetFieldValue<DateTime>(reader.GetOrdinal("created_at"))
            ));
        }

        return groups;
    }

    public async Task<List<GroupResponse>> GetByUserIdAsync(int userId)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            @"SELECT g.id, g.name, g.description, g.created_by, g.created_at 
              FROM groups g 
              INNER JOIN group_members gm ON g.id = gm.group_id 
              WHERE gm.user_id = @userId", connection);

        command.Parameters.Add(new NpgsqlParameter("@userId", NpgsqlDbType.Integer) { Value = userId });

        var groups = new List<GroupResponse>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            groups.Add(new GroupResponse(
                reader.GetFieldValue<int>(reader.GetOrdinal("id")),
                reader.GetFieldValue<string>(reader.GetOrdinal("name")),
                reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetFieldValue<string>(reader.GetOrdinal("description")),
                reader.IsDBNull(reader.GetOrdinal("created_by")) ? 0 : reader.GetFieldValue<int>(reader.GetOrdinal("created_by")),
                reader.GetFieldValue<DateTime>(reader.GetOrdinal("created_at"))
            ));
        }

        return groups;
    }

    public async Task<int> CreateAsync(Group group)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            await using var command = new NpgsqlCommand(
                @"INSERT INTO groups (name, description, created_by, created_at) 
                  VALUES (@name, @description, @createdBy, @createdAt) 
                  RETURNING id", connection, transaction);

            command.Parameters.Add(new NpgsqlParameter("@name", NpgsqlDbType.Varchar) { Value = group.Name });
            command.Parameters.Add(new NpgsqlParameter("@description", NpgsqlDbType.Text) { Value = (object?)group.Description ?? DBNull.Value });
            command.Parameters.Add(new NpgsqlParameter("@createdBy", NpgsqlDbType.Integer) { Value = (object?)group.CreatedBy ?? DBNull.Value });
            command.Parameters.Add(new NpgsqlParameter("@createdAt", NpgsqlDbType.TimestampTz) { Value = group.CreatedAt });

            var id = (int)(await command.ExecuteScalarAsync())!;

            // Add creator as member
            await using var memberCommand = new NpgsqlCommand(
                "INSERT INTO group_members (group_id, user_id, joined_at) VALUES (@groupId, @userId, @joinedAt)", connection, transaction);

            memberCommand.Parameters.Add(new NpgsqlParameter("@groupId", NpgsqlDbType.Integer) { Value = id });
            memberCommand.Parameters.Add(new NpgsqlParameter("@userId", NpgsqlDbType.Integer) { Value = group.CreatedBy });
            memberCommand.Parameters.Add(new NpgsqlParameter("@joinedAt", NpgsqlDbType.TimestampTz) { Value = DateTime.UtcNow });

            await memberCommand.ExecuteNonQueryAsync();

            await transaction.CommitAsync();

            return id;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Group?> GetByIdAsync(int id)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            "SELECT id, name, description, created_by, created_at FROM groups WHERE id = @id", connection);

        command.Parameters.Add(new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = id });

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Group
            {
                Id = reader.GetFieldValue<int>(reader.GetOrdinal("id")),
                Name = reader.GetFieldValue<string>(reader.GetOrdinal("name")),
                Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetFieldValue<string>(reader.GetOrdinal("description")),
                CreatedBy = reader.IsDBNull(reader.GetOrdinal("created_by")) ? null : reader.GetFieldValue<int?>(reader.GetOrdinal("created_by")),
                CreatedAt = reader.GetFieldValue<DateTime>(reader.GetOrdinal("created_at"))
            };
        }

        return null;
    }

    public async Task<List<GroupMemberResponse>> GetMembersAsync(int groupId)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            @"SELECT u.id, u.first_name, u.last_name, gm.joined_at 
              FROM group_members gm 
              INNER JOIN users u ON gm.user_id = u.id 
              WHERE gm.group_id = @groupId", connection);

        command.Parameters.Add(new NpgsqlParameter("@groupId", NpgsqlDbType.Integer) { Value = groupId });

        var members = new List<GroupMemberResponse>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            members.Add(new GroupMemberResponse(
                reader.GetFieldValue<int>(reader.GetOrdinal("id")),
                reader.GetFieldValue<string>(reader.GetOrdinal("first_name")),
                reader.GetFieldValue<string>(reader.GetOrdinal("last_name")),
                reader.GetFieldValue<DateTime>(reader.GetOrdinal("joined_at"))
            ));
        }

        return members;
    }

    public async Task<bool> IsMemberAsync(int groupId, int userId)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            "SELECT COUNT(1) FROM group_members WHERE group_id = @groupId AND user_id = @userId", connection);

        command.Parameters.Add(new NpgsqlParameter("@groupId", NpgsqlDbType.Integer) { Value = groupId });
        command.Parameters.Add(new NpgsqlParameter("@userId", NpgsqlDbType.Integer) { Value = userId });

        var count = (long)(await command.ExecuteScalarAsync())!;
        return count > 0;
    }

    public async Task JoinGroupAsync(int groupId, int userId)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            "INSERT INTO group_members (group_id, user_id, joined_at) VALUES (@groupId, @userId, @joinedAt) ON CONFLICT DO NOTHING", connection);

        command.Parameters.Add(new NpgsqlParameter("@groupId", NpgsqlDbType.Integer) { Value = groupId });
        command.Parameters.Add(new NpgsqlParameter("@userId", NpgsqlDbType.Integer) { Value = userId });
        command.Parameters.Add(new NpgsqlParameter("@joinedAt", NpgsqlDbType.TimestampTz) { Value = DateTime.UtcNow });

        await command.ExecuteNonQueryAsync();
    }

    public async Task AddMemberAsync(int groupId, int userId)
    {
        await JoinGroupAsync(groupId, userId);
    }

    public async Task LeaveGroupAsync(int groupId, int userId)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            "DELETE FROM group_members WHERE group_id = @groupId AND user_id = @userId", connection);

        command.Parameters.Add(new NpgsqlParameter("@groupId", NpgsqlDbType.Integer) { Value = groupId });
        command.Parameters.Add(new NpgsqlParameter("@userId", NpgsqlDbType.Integer) { Value = userId });

        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<GroupMessageResponse>> GetMessagesAsync(int groupId, int page, int limit)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        var offset = (page - 1) * limit;

        await using var command = new NpgsqlCommand(
            @"SELECT gm.id, gm.group_id, gm.sender_id, 
                     u.first_name || ' ' || u.last_name as sender_name,
                     gm.content, gm.sent_at
              FROM group_messages gm 
              INNER JOIN users u ON gm.sender_id = u.id 
              WHERE gm.group_id = @groupId 
              ORDER BY gm.sent_at ASC 
              LIMIT @limit OFFSET @offset", connection);

        command.Parameters.Add(new NpgsqlParameter("@groupId", NpgsqlDbType.Integer) { Value = groupId });
        command.Parameters.Add(new NpgsqlParameter("@limit", NpgsqlDbType.Integer) { Value = limit });
        command.Parameters.Add(new NpgsqlParameter("@offset", NpgsqlDbType.Integer) { Value = offset });

        var messages = new List<GroupMessageResponse>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            messages.Add(new GroupMessageResponse(
                reader.GetFieldValue<int>(reader.GetOrdinal("id")),
                reader.GetFieldValue<int>(reader.GetOrdinal("group_id")),
                reader.GetFieldValue<int>(reader.GetOrdinal("sender_id")),
                reader.GetFieldValue<string>(reader.GetOrdinal("sender_name")),
                reader.GetFieldValue<string>(reader.GetOrdinal("content")),
                reader.GetFieldValue<DateTime>(reader.GetOrdinal("sent_at"))
            ));
        }

        return messages;
    }

    public async Task<GroupMessageResponse> SendMessageAsync(int groupId, int userId, string content)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            @"INSERT INTO group_messages (group_id, sender_id, content, sent_at) 
              VALUES (@groupId, @senderId, @content, @sentAt) 
              RETURNING id, sent_at", connection);

        command.Parameters.Add(new NpgsqlParameter("@groupId", NpgsqlDbType.Integer) { Value = groupId });
        command.Parameters.Add(new NpgsqlParameter("@senderId", NpgsqlDbType.Integer) { Value = userId });
        command.Parameters.Add(new NpgsqlParameter("@content", NpgsqlDbType.Text) { Value = content });
        command.Parameters.Add(new NpgsqlParameter("@sentAt", NpgsqlDbType.TimestampTz) { Value = DateTime.UtcNow });

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            throw new InvalidOperationException("Failed to send message");
        }

        var id = reader.GetFieldValue<int>(reader.GetOrdinal("id"));
        var sentAt = reader.GetFieldValue<DateTime>(reader.GetOrdinal("sent_at"));

        // Get sender name
        await using var userCommand = new NpgsqlCommand(
            "SELECT first_name || ' ' || last_name FROM users WHERE id = @userId", connection);
        userCommand.Parameters.Add(new NpgsqlParameter("@userId", NpgsqlDbType.Integer) { Value = userId });

        var senderName = (string)(await userCommand.ExecuteScalarAsync())!;

        return new GroupMessageResponse(id, groupId, userId, senderName, content, sentAt);
    }
}
