using CarAuction.Application.DTOs.Listings;
using CarAuction.Application.DTOs.Users;

namespace CarAuction.Application.Interfaces.Services;

public interface IListingService
{
    Task<PaginatedResponse<ListingListItemResponse>> GetAllAsync(ListingFilters filters);
    Task<ListingDetailResponse?> GetByIdAsync(int id);
    Task<ListingDetailResponse> CreateAsync(int userId, CreateListingRequest request);
    Task<ListingDetailResponse> UpdateAsync(int id, int userId, UpdateListingRequest request);
    Task DeleteAsync(int id, int userId);
    Task<List<ListingListItemResponse>> GetMyListingsAsync(int userId);
}
