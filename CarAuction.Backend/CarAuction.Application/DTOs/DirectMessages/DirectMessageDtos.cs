namespace CarAuction.Application.DTOs.DirectMessages;

public record StartConversationRequest(
    int TargetUserId
);

public record ConversationResponse(
    int Id,
    int User1Id,
    int User2Id,
    string OtherUserFirstName,
    string OtherUserLastName,
    string? LastMessage,
    bool? IsRead,
    DateTime CreatedAt
);

public record SendMessageRequest(
    string Content
);

public record MessageResponse(
    int Id,
    int ConversationId,
    int SenderId,
    string Content,
    DateTime SentAt,
    bool IsRead
);
