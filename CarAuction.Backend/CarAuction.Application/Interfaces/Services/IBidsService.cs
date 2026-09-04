using CarAuction.Application.DTOs.Bids;

namespace CarAuction.Application.Interfaces.Services;

public interface IBidsService
{
    Task<OfferResponse> MakeOfferAsync(int listingId, int userId, MakeOfferRequest request);
    Task<List<OfferResponse>> GetOffersAsync(int listingId, int userId);
    Task<List<MyBidResponse>> GetMyBidsAsync(int userId);
}
