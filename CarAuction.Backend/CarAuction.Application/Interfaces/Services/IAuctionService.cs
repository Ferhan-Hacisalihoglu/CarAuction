using CarAuction.Application.DTOs.Auctions;

namespace CarAuction.Application.Interfaces.Services;

public interface IAuctionService
{
    Task<List<AuctionListItemResponse>> GetActiveAuctionsAsync();
    Task<AuctionDetailResponse?> GetByIdAsync(int id);
    Task<AuctionDetailResponse?> GetByListingIdAsync(int listingId);
    Task<AuctionDetailResponse> PlaceBidAsync(int auctionId, int userId, PlaceBidRequest request, string? idempotencyKey);
    Task<List<BidHistoryResponse>> GetBidHistoryAsync(int auctionId);
}
