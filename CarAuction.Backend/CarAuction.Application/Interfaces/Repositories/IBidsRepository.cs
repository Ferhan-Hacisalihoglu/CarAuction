using CarAuction.Application.DTOs.Bids;

namespace CarAuction.Application.Interfaces.Repositories;

public interface IBidsRepository
{
    Task<OfferResponse> MakeOfferAsync(int listingId, int userId, decimal amount);
    Task<List<OfferResponse>> GetOffersByListingIdAsync(int listingId);
    Task<List<MyBidResponse>> GetMyBidsAsync(int userId);
    Task<bool> IsListingOwnerAsync(int listingId, int userId);
    Task<long> CountRecentAsync(TimeSpan period);
    Task<List<BidInfo>> GetRecentAsync(int limit);
    Task UpdateBidStatusAsync(int bidId, string status);
    Task<int> GetListingIdByBidIdAsync(int bidId);
    Task<OfferResponse?> GetOfferByIdAsync(int id);
}
