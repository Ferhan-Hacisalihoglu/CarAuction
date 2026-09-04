using CarAuction.Application.DTOs.DirectMessages;
using CarAuction.Application.DTOs.Groups;

namespace CarAuction.Application.Interfaces.Hubs;

public interface IChatHubClient
{
    Task ReceiveDirectMessage(MessageResponse message);
    Task ReceiveGroupMessage(GroupMessageResponse message);
    Task UserTyping(int senderId, string senderName, int targetId, bool isGroup);
    Task MessageReadReceipt(int conversationId, int messageId);
}
