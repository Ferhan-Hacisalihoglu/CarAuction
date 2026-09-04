using CarAuction.Application.DTOs.DirectMessages;
using CarAuction.Application.Interfaces.Repositories;
using CarAuction.Infrastructure.Data.Connection;
using Npgsql;
using NpgsqlTypes;

namespace CarAuction.Infrastructure.Repositories;

public class DirectMessageRepository : IDirectMessageRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public DirectMessageRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<ConversationResponse>> GetConversationsAsync(int userId)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            @"SELECT c.id, c.user1_id, c.user2_id, c.created_at,
                     u.first_name as other_first_name, u.last_name as other_last_name,
                     (SELECT content FROM messages WHERE conversation_id = c.id ORDER BY sent_at DESC LIMIT 1) as last_message,
                     (SELECT is_read FROM messages WHERE conversation_id = c.id AND sender_id != @userId ORDER BY sent_at DESC LIMIT 1) as is_read
              FROM conversations c
              INNER JOIN users u ON (CASE WHEN c.user1_id = @userId THEN c.user2_id ELSE c.user1_id END) = u.id
              WHERE c.user1_id = @userId OR c.user2_id = @userId
              ORDER BY c.created_at DESC", connection);

        command.Parameters.Add(new NpgsqlParameter("@userId", NpgsqlDbType.Integer) { Value = userId });

        var conversations = new List<ConversationResponse>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            conversations.Add(new ConversationResponse(
                reader.GetFieldValue<int>(reader.GetOrdinal("id")),
                reader.GetFieldValue<int>(reader.GetOrdinal("user1_id")),
                reader.GetFieldValue<int>(reader.GetOrdinal("user2_id")),
                reader.GetFieldValue<string>(reader.GetOrdinal("other_first_name")),
                reader.GetFieldValue<string>(reader.GetOrdinal("other_last_name")),
                reader.IsDBNull(reader.GetOrdinal("last_message")) ? null : reader.GetFieldValue<string>(reader.GetOrdinal("last_message")),
                reader.IsDBNull(reader.GetOrdinal("is_read")) ? null : reader.GetFieldValue<bool?>(reader.GetOrdinal("is_read")),
                reader.GetFieldValue<DateTime>(reader.GetOrdinal("created_at"))
            ));
        }

        return conversations;
    }

    public async Task<int> StartConversationAsync(int userId, int targetUserId)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        var u1 = Math.Min(userId, targetUserId);
        var u2 = Math.Max(userId, targetUserId);

        // Check if conversation exists
        await using var checkCommand = new NpgsqlCommand(
            "SELECT id FROM conversations WHERE user1_id = @u1 AND user2_id = @u2", connection);

        checkCommand.Parameters.Add(new NpgsqlParameter("@u1", NpgsqlDbType.Integer) { Value = u1 });
        checkCommand.Parameters.Add(new NpgsqlParameter("@u2", NpgsqlDbType.Integer) { Value = u2 });

        var existingId = await checkCommand.ExecuteScalarAsync();
        if (existingId != null && existingId != DBNull.Value)
        {
            return (int)existingId;
        }

        // Create new conversation
        await using var command = new NpgsqlCommand(
            @"INSERT INTO conversations (user1_id, user2_id, created_at) 
              VALUES (@u1, @u2, @createdAt) 
              ON CONFLICT (user1_id, user2_id) DO UPDATE SET created_at = conversations.created_at 
              RETURNING id", connection);

        command.Parameters.Add(new NpgsqlParameter("@u1", NpgsqlDbType.Integer) { Value = u1 });
        command.Parameters.Add(new NpgsqlParameter("@u2", NpgsqlDbType.Integer) { Value = u2 });
        command.Parameters.Add(new NpgsqlParameter("@createdAt", NpgsqlDbType.TimestampTz) { Value = DateTime.UtcNow });

        var id = (int)(await command.ExecuteScalarAsync())!;
        return id;
    }

    public async Task<bool> IsParticipantAsync(int conversationId, int userId)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            "SELECT COUNT(1) FROM conversations WHERE id = @id AND (user1_id = @userId OR user2_id = @userId)", connection);

        command.Parameters.Add(new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = conversationId });
        command.Parameters.Add(new NpgsqlParameter("@userId", NpgsqlDbType.Integer) { Value = userId });

        var count = (long)(await command.ExecuteScalarAsync())!;
        return count > 0;
    }

    public async Task<List<MessageResponse>> GetMessagesAsync(int conversationId, int page, int limit)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        var offset = (page - 1) * limit;

        await using var command = new NpgsqlCommand(
            @"SELECT id, conversation_id, sender_id, content, sent_at, is_read
              FROM messages 
              WHERE conversation_id = @conversationId 
              ORDER BY sent_at ASC 
              LIMIT @limit OFFSET @offset", connection);

        command.Parameters.Add(new NpgsqlParameter("@conversationId", NpgsqlDbType.Integer) { Value = conversationId });
        command.Parameters.Add(new NpgsqlParameter("@limit", NpgsqlDbType.Integer) { Value = limit });
        command.Parameters.Add(new NpgsqlParameter("@offset", NpgsqlDbType.Integer) { Value = offset });

        var messages = new List<MessageResponse>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            messages.Add(new MessageResponse(
                reader.GetFieldValue<int>(reader.GetOrdinal("id")),
                reader.GetFieldValue<int>(reader.GetOrdinal("conversation_id")),
                reader.GetFieldValue<int>(reader.GetOrdinal("sender_id")),
                reader.GetFieldValue<string>(reader.GetOrdinal("content")),
                reader.GetFieldValue<DateTime>(reader.GetOrdinal("sent_at")),
                reader.GetFieldValue<bool>(reader.GetOrdinal("is_read"))
            ));
        }

        return messages;
    }

    public async Task<MessageResponse> SendMessageAsync(int conversationId, int userId, string content)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            @"INSERT INTO messages (conversation_id, sender_id, content, sent_at, is_read) 
              VALUES (@conversationId, @senderId, @content, @sentAt, FALSE) 
              RETURNING id, sent_at", connection);

        command.Parameters.Add(new NpgsqlParameter("@conversationId", NpgsqlDbType.Integer) { Value = conversationId });
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

        return new MessageResponse(id, conversationId, userId, content, sentAt, false);
    }

    public async Task MarkAsReadAsync(int conversationId, int userId)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            @"UPDATE messages SET is_read = TRUE 
              WHERE conversation_id = @conversationId AND sender_id != @userId AND is_read = FALSE", connection);

        command.Parameters.Add(new NpgsqlParameter("@conversationId", NpgsqlDbType.Integer) { Value = conversationId });
        command.Parameters.Add(new NpgsqlParameter("@userId", NpgsqlDbType.Integer) { Value = userId });

        await command.ExecuteNonQueryAsync();
    }

    public async Task<int?> GetOtherParticipantIdAsync(int conversationId, int userId)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            @"SELECT CASE WHEN user1_id = @userId THEN user2_id ELSE user1_id END as other_user_id
              FROM conversations 
              WHERE id = @conversationId AND (user1_id = @userId OR user2_id = @userId)", connection);

        command.Parameters.Add(new NpgsqlParameter("@conversationId", NpgsqlDbType.Integer) { Value = conversationId });
        command.Parameters.Add(new NpgsqlParameter("@userId", NpgsqlDbType.Integer) { Value = userId });

        var result = await command.ExecuteScalarAsync();
        if (result != null && result != DBNull.Value)
        {
            return Convert.ToInt32(result);
        }

        return null;
    }
}
