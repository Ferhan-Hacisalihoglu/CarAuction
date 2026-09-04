using CarAuction.Application.DTOs.Auctions;
using CarAuction.Application.DTOs.DirectMessages;
using CarAuction.Application.DTOs.Groups;

namespace CarAuction.Application.Interfaces.Services;

public interface INotificationService
{
    Task NotifyNewBidAsync(int auctionId, AuctionHubDto bid);
    Task NotifyAuctionEndedAsync(int auctionId, AuctionResultDto result);
    Task NotifyAuctionTimeExtendedAsync(int auctionId, AuctionExtendedDto extension);
    Task NotifyDirectMessageAsync(int recipientUserId, MessageResponse message);
    Task NotifyGroupMessageAsync(int groupId, GroupMessageResponse message);
    Task NotifyMessageReadReceiptAsync(int conversationId, int messageId, int recipientUserId);
}
