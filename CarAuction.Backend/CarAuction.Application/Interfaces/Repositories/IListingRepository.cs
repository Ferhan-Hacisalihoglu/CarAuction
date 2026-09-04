using CarAuction.Application.DTOs.Listings;
using CarAuction.Domain.Entities;

namespace CarAuction.Application.Interfaces.Repositories;

public interface IListingRepository
{
    Task<(List<Listing> Listings, int Total)> GetPaginatedAsync(ListingFilters filters);
    Task<Listing?> GetByIdAsync(int id);
    Task<Listing?> GetByIdWithImagesAsync(int id);
    Task<int> CreateAsync(Listing listing);
    Task CreateAuctionAsync(Auction auction);
    Task UpdateAsync(Listing listing);
    Task SoftDeleteAsync(int id);
    Task<bool> IsOwnerAsync(int id, int userId);
    Task<List<Listing>> GetByUserIdAsync(int userId);
    Task<List<Image>> GetImagesByListingIdAsync(int listingId);
    Task AddImageAsync(Image image);
}
