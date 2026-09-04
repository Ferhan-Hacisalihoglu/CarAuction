using CarAuction.Application.DTOs.DirectMessages;

namespace CarAuction.Application.Interfaces.Services;

public interface IDirectMessageService
{
    Task<List<ConversationResponse>> GetConversationsAsync(int userId);
    Task<ConversationResponse> StartConversationAsync(int userId, StartConversationRequest request);
    Task<List<MessageResponse>> GetMessagesAsync(int conversationId, int userId, int page, int limit);
    Task<MessageResponse> SendMessageAsync(int conversationId, int userId, SendMessageRequest request);
    Task MarkAsReadAsync(int conversationId, int userId);
}
