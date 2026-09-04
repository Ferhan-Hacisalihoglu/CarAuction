using CarAuction.Application.DTOs.Auctions;
using CarAuction.Application.DTOs.DirectMessages;
using CarAuction.Application.DTOs.Groups;
using CarAuction.Application.Interfaces.Services;
using CarAuction.Presentation.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CarAuction.Presentation.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<AuctionHub> _auctionHubContext;
    private readonly IHubContext<ChatHub> _chatHubContext;

    public NotificationService(
        IHubContext<AuctionHub> auctionHubContext,
        IHubContext<ChatHub> chatHubContext)
    {
        _auctionHubContext = auctionHubContext;
        _chatHubContext = chatHubContext;
    }

    public async Task NotifyNewBidAsync(int auctionId, AuctionHubDto bid)
    {
        await _auctionHubContext.Clients.Group($"Auction_{auctionId}").SendAsync("ReceiveNewBid", bid);
    }

    public async Task NotifyAuctionEndedAsync(int auctionId, AuctionResultDto result)
    {
        await _auctionHubContext.Clients.Group($"Auction_{auctionId}").SendAsync("AuctionEnded", result);
    }

    public async Task NotifyAuctionTimeExtendedAsync(int auctionId, AuctionExtendedDto extension)
    {
        await _auctionHubContext.Clients.Group($"Auction_{auctionId}").SendAsync("AuctionTimeExtended", extension);
    }

    public async Task NotifyDirectMessageAsync(int recipientUserId, MessageResponse message)
    {
        await _chatHubContext.Clients.Group($"User_{recipientUserId}").SendAsync("ReceiveDirectMessage", message);
    }

    public async Task NotifyGroupMessageAsync(int groupId, GroupMessageResponse message)
    {
        await _chatHubContext.Clients.Group($"Group_{groupId}").SendAsync("ReceiveGroupMessage", message);
    }

    public async Task NotifyMessageReadReceiptAsync(int conversationId, int messageId, int recipientUserId)
    {
        await _chatHubContext.Clients.Group($"User_{recipientUserId}").SendAsync("MessageReadReceipt", conversationId, messageId);
    }
}
