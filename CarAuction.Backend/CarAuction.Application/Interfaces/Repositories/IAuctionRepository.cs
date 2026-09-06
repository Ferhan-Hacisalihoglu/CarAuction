using CarAuction.Application.DTOs.Auctions;

namespace CarAuction.Application.Interfaces.Repositories;

public interface IAuctionRepository
{
    Task<List<AuctionListItemResponse>> GetActiveAuctionsAsync();
    Task<AuctionDetailResponse?> GetByIdAsync(int id);
    Task<AuctionDetailResponse?> GetByListingIdAsync(int listingId);
    Task<AuctionDetailResponse> PlaceBidWithTransactionAsync(int auctionId, int userId, decimal amount, string? idempotencyKey);
    Task<List<BidHistoryResponse>> GetBidHistoryAsync(int auctionId);
    Task<bool> IdempotencyKeyExistsAsync(string key);
    Task<long> CountActiveAsync();
}
