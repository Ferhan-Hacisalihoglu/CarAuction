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

    public async Task<OfferResponse> AcceptOfferAsync(int offerId, int userId)
    {
        // Get the offer to find the listing
        var listingId = await _bidsRepository.GetListingIdByBidIdAsync(offerId);
        
        // Verify the caller is the listing owner
        if (!await _bidsRepository.IsListingOwnerAsync(listingId, userId))
        {
            throw new UnauthorizedAccessException("Only the listing owner can accept offers");
        }

        // Update the bid status to accepted
        await _bidsRepository.UpdateBidStatusAsync(offerId, "accepted");

        // Mark listing as sold
        // Reject all other pending offers for this listing
        var offers = await _bidsRepository.GetOffersByListingIdAsync(listingId);
        foreach (var offer in offers.Where(o => o.Id != offerId))
        {
            await _bidsRepository.UpdateBidStatusAsync(offer.Id, "rejected");
        }

        // Return the updated offer
        var updatedOffer = await _bidsRepository.GetOfferByIdAsync(offerId);
        return updatedOffer ?? throw new InvalidOperationException("Offer not found after acceptance");
    }

    public async Task RejectOfferAsync(int offerId, int userId)
    {
        var listingId = await _bidsRepository.GetListingIdByBidIdAsync(offerId);
        
        if (!await _bidsRepository.IsListingOwnerAsync(listingId, userId))
        {
            throw new UnauthorizedAccessException("Only the listing owner can reject offers");
        }

        await _bidsRepository.UpdateBidStatusAsync(offerId, "rejected");
    }
}
