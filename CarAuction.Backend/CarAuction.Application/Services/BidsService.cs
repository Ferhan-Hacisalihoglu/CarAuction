using CarAuction.Application.DTOs.Bids;
using CarAuction.Application.Interfaces.Repositories;
using CarAuction.Application.Interfaces.Services;

namespace CarAuction.Application.Services;

public class BidsService : IBidsService
{
    private readonly IBidsRepository _bidsRepository;

    public BidsService(IBidsRepository bidsRepository)
    {
        _bidsRepository = bidsRepository;
    }

    public async Task<OfferResponse> MakeOfferAsync(int listingId, int userId, MakeOfferRequest request)
    {
        // Check if user is the listing owner
        if (await _bidsRepository.IsListingOwnerAsync(listingId, userId))
        {
            throw new InvalidOperationException("You cannot make an offer on your own listing");
        }

        return await _bidsRepository.MakeOfferAsync(listingId, userId, request.Amount);
    }

    public async Task<List<OfferResponse>> GetOffersAsync(int listingId, int userId)
    {
        // Check if user is the listing owner (only owner can view offers)
        if (!await _bidsRepository.IsListingOwnerAsync(listingId, userId))
        {
            throw new UnauthorizedAccessException("You are not the owner of this listing");
        }

        return await _bidsRepository.GetOffersByListingIdAsync(listingId);
    }

    public async Task<List<MyBidResponse>> GetMyBidsAsync(int userId)
    {
        return await _bidsRepository.GetMyBidsAsync(userId);
    }
}
