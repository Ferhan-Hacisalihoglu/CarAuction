using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace CarAuction.Presentation.Hubs;

[Authorize]
public class ChatHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId > 0)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
        }
        await base.OnConnectedAsync();
    }

    public async Task JoinGroup(int groupId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Group_{groupId}");
    }

    public async Task LeaveGroup(int groupId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Group_{groupId}");
    }

    public async Task SendDirectMessage(int conversationId, int recipientId, string content)
    {
        var senderId = GetUserId();
        await Clients.Group($"User_{recipientId}").SendAsync("ReceiveDirectMessage", new
        {
            Id = 0,
            ConversationId = conversationId,
            SenderId = senderId,
            Content = content,
            SentAt = DateTime.UtcNow,
            IsRead = false
        });
    }

    public async Task SendGroupMessage(int groupId, string content)
    {
        var senderId = GetUserId();
        var senderName = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? $"User #{senderId}";

        await Clients.Group($"Group_{groupId}").SendAsync("ReceiveGroupMessage", new
        {
            Id = 0,
            GroupId = groupId,
            SenderId = senderId,
            SenderName = senderName,
            Content = content,
            SentAt = DateTime.UtcNow
        });
    }

    public async Task Typing(int targetId, bool isGroup)
    {
        var senderId = GetUserId();
        var senderName = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? $"User #{senderId}";

        if (isGroup)
        {
            await Clients.Group($"Group_{targetId}").SendAsync("UserTyping", senderId, senderName, targetId, true);
        }
        else
        {
            await Clients.Group($"User_{targetId}").SendAsync("UserTyping", senderId, senderName, targetId, false);
        }
    }

    private int GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        return 0;
    }
}
