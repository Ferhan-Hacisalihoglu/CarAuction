using CarAuction.Application.DTOs.Auctions;

namespace CarAuction.Application.Interfaces.Hubs;

public interface IAuctionHubClient
{
    Task ReceiveNewBid(AuctionHubDto bid);
    Task AuctionEnded(AuctionResultDto result);
    Task AuctionTimeExtended(AuctionExtendedDto extension);
}
