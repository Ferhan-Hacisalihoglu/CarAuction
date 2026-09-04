using CarAuction.Application.DTOs.DirectMessages;

namespace CarAuction.Application.Interfaces.Repositories;

public interface IDirectMessageRepository
{
    Task<List<ConversationResponse>> GetConversationsAsync(int userId);
    Task<int> StartConversationAsync(int userId, int targetUserId);
    Task<bool> IsParticipantAsync(int conversationId, int userId);
    Task<int?> GetOtherParticipantIdAsync(int conversationId, int userId);
    Task<List<MessageResponse>> GetMessagesAsync(int conversationId, int page, int limit);
    Task<MessageResponse> SendMessageAsync(int conversationId, int userId, string content);
    Task MarkAsReadAsync(int conversationId, int userId);
}
