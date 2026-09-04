using CarAuction.Application.DTOs.DirectMessages;
using CarAuction.Application.Interfaces.Repositories;
using CarAuction.Application.Interfaces.Services;

namespace CarAuction.Application.Services;

public class DirectMessageService : IDirectMessageService
{
    private readonly IDirectMessageRepository _repository;
    private readonly INotificationService _notificationService;

    public DirectMessageService(IDirectMessageRepository repository, INotificationService notificationService)
    {
        _repository = repository;
        _notificationService = notificationService;
    }

    public async Task<List<ConversationResponse>> GetConversationsAsync(int userId)
    {
        return await _repository.GetConversationsAsync(userId);
    }

    public async Task<ConversationResponse> StartConversationAsync(int userId, StartConversationRequest request)
    {
        if (userId == request.TargetUserId)
        {
            throw new InvalidOperationException("You cannot start a conversation with yourself");
        }

        var conversationId = await _repository.StartConversationAsync(userId, request.TargetUserId);

        var conversations = await _repository.GetConversationsAsync(userId);
        var conversation = conversations.FirstOrDefault(c => c.Id == conversationId);

        if (conversation == null)
        {
            throw new InvalidOperationException("Failed to retrieve conversation after creation");
        }

        return conversation;
    }

    public async Task<List<MessageResponse>> GetMessagesAsync(int conversationId, int userId, int page, int limit)
    {
        if (!await _repository.IsParticipantAsync(conversationId, userId))
        {
            throw new UnauthorizedAccessException("You are not a participant of this conversation");
        }

        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 100);

        return await _repository.GetMessagesAsync(conversationId, page, limit);
    }

    public async Task<MessageResponse> SendMessageAsync(int conversationId, int userId, SendMessageRequest request)
    {
        if (!await _repository.IsParticipantAsync(conversationId, userId))
        {
            throw new UnauthorizedAccessException("You are not a participant of this conversation");
        }

        var message = await _repository.SendMessageAsync(conversationId, userId, request.Content);

        // Broadcast to recipient via ChatHub
        var recipientId = await _repository.GetOtherParticipantIdAsync(conversationId, userId);
        if (recipientId.HasValue)
        {
            await _notificationService.NotifyDirectMessageAsync(recipientId.Value, message);
        }

        return message;
    }

    public async Task MarkAsReadAsync(int conversationId, int userId)
    {
        if (!await _repository.IsParticipantAsync(conversationId, userId))
        {
            throw new UnauthorizedAccessException("You are not a participant of this conversation");
        }

        await _repository.MarkAsReadAsync(conversationId, userId);

        // Broadcast read receipt to other participant
        var otherUserId = await _repository.GetOtherParticipantIdAsync(conversationId, userId);
        if (otherUserId.HasValue)
        {
            await _notificationService.NotifyMessageReadReceiptAsync(conversationId, 0, otherUserId.Value);
        }
    }
}
