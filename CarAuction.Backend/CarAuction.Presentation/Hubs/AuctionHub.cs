using CarAuction.Application.DTOs.Auctions;
using Microsoft.AspNetCore.SignalR;

namespace CarAuction.Presentation.Hubs;

public class AuctionHub : Hub
{
    public async Task JoinAuction(int auctionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Auction_{auctionId}");
    }

    public async Task LeaveAuction(int auctionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Auction_{auctionId}");
    }
}
