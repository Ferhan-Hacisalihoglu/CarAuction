namespace CarAuction.Application.DTOs.Groups;

public record CreateGroupRequest(
    string Name,
    string? Description
);

public record GroupResponse(
    int Id,
    string Name,
    string? Description,
    int CreatedBy,
    DateTime CreatedAt
);

public record GroupDetailResponse(
    int Id,
    string Name,
    string? Description,
    int CreatedBy,
    DateTime CreatedAt,
    List<GroupMemberResponse> Members
);

public record GroupMemberResponse(
    int UserId,
    string FirstName,
    string LastName,
    DateTime JoinedAt
);

public record SendMessageRequest(
    string Content
);

public record GroupMessageResponse(
    int Id,
    int GroupId,
    int SenderId,
    string SenderName,
    string Content,
    DateTime SentAt
);
