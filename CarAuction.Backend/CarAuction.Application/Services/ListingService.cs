using CarAuction.Application.DTOs.Listings;
using CarAuction.Application.DTOs.Users;
using CarAuction.Application.Interfaces.Caching;
using CarAuction.Application.Interfaces.Repositories;
using CarAuction.Application.Interfaces.Services;
using CarAuction.Domain.Entities;

namespace CarAuction.Application.Services;

public class ListingService : IListingService
{
    private readonly IListingRepository _listingRepository;
    private readonly ICacheService _cacheService;

    public ListingService(IListingRepository listingRepository, ICacheService cacheService)
    {
        _listingRepository = listingRepository;
        _cacheService = cacheService;
    }

    public async Task<PaginatedResponse<ListingListItemResponse>> GetAllAsync(ListingFilters filters)
    {
        // Validate pagination parameters
        var page = Math.Max(1, filters.Page);
        var limit = Math.Clamp(filters.Limit, 1, 100);
        var cacheKey = $"listings:catalog:p{page}:l{limit}:q{filters.Search}:s{filters.Status}:min{filters.MinPrice}:max{filters.MaxPrice}:auc{filters.IsAuction}";

        return await _cacheService.GetOrSetAsync(cacheKey, async () =>
        {
            var (listings, total) = await _listingRepository.GetPaginatedAsync(filters with { Page = page, Limit = limit });
            var totalPages = (int)Math.Ceiling((double)total / limit);

            return new PaginatedResponse<ListingListItemResponse>(
                listings.Select(MapToListItem).ToList(),
                total,
                filters.Page,
                filters.Limit,
                totalPages
            );
        }, TimeSpan.FromSeconds(60));
    }

    public async Task<ListingDetailResponse?> GetByIdAsync(int id)
    {
        var cacheKey = $"listing:{id}";
        var cached = await _cacheService.GetAsync<ListingDetailResponse>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        var listing = await _listingRepository.GetByIdWithImagesAsync(id);
        if (listing == null)
        {
            return null;
        }

        var response = MapToDetailResponse(listing);
        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));
        return response;
    }

    public async Task<ListingDetailResponse> CreateAsync(int userId, CreateListingRequest request)
    {
        var listing = new Listing
        {
            UserId = userId,
            Title = request.Title,
            Description = request.Description,
            Price = request.Price,
            IsAuction = request.IsAuction,
            Status = "active",
            CreatedAt = DateTime.UtcNow
        };

        var listingId = await _listingRepository.CreateAsync(listing);

        // If auction listing, create auction record
        if (request.IsAuction && request.StartingPrice.HasValue && request.StartTime.HasValue && request.EndTime.HasValue && request.MinBidIncrement.HasValue)
        {
            var auction = new Auction
            {
                ListingId = listingId,
                StartingPrice = request.StartingPrice.Value,
                CurrentPrice = request.StartingPrice.Value,
                StartTime = request.StartTime.Value,
                EndTime = request.EndTime.Value,
                MinBidIncrement = request.MinBidIncrement.Value,
                Status = "active"
            };

            await _listingRepository.CreateAuctionAsync(auction);
            await _cacheService.RemoveAsync("auctions:active");
        }

        var createdListing = await _listingRepository.GetByIdWithImagesAsync(listingId);
        if (createdListing == null)
        {
            throw new InvalidOperationException("Listing not found after creation");
        }

        var response = MapToDetailResponse(createdListing);
        await _cacheService.SetAsync($"listing:{listingId}", response, TimeSpan.FromMinutes(10));
        await _cacheService.RemoveByPrefixAsync("listings:catalog");

        return response;
    }

    public async Task<ListingDetailResponse> UpdateAsync(int id, int userId, UpdateListingRequest request)
    {
        // Check ownership
        if (!await _listingRepository.IsOwnerAsync(id, userId))
        {
            throw new UnauthorizedAccessException("You are not the owner of this listing");
        }

        var listing = await _listingRepository.GetByIdAsync(id);
        if (listing == null)
        {
            throw new KeyNotFoundException($"Listing with ID {id} not found");
        }

        listing.Title = request.Title;
        listing.Description = request.Description;
        listing.Price = request.Price;
        listing.UpdatedAt = DateTime.UtcNow;

        await _listingRepository.UpdateAsync(listing);

        var updatedListing = await _listingRepository.GetByIdWithImagesAsync(id);
        if (updatedListing == null)
        {
            throw new InvalidOperationException("Listing not found after update");
        }

        var response = MapToDetailResponse(updatedListing);
        // Invalidate and update Redis cache
        await _cacheService.SetAsync($"listing:{id}", response, TimeSpan.FromMinutes(10));
        await _cacheService.RemoveByPrefixAsync("listings:catalog");
        await _cacheService.RemoveAsync("auctions:active");

        return response;
    }

    public async Task DeleteAsync(int id, int userId)
    {
        // Check ownership
        if (!await _listingRepository.IsOwnerAsync(id, userId))
        {
            throw new UnauthorizedAccessException("You are not the owner of this listing");
        }

        var listing = await _listingRepository.GetByIdAsync(id);
        if (listing == null)
        {
            throw new KeyNotFoundException($"Listing with ID {id} not found");
        }

        await _listingRepository.SoftDeleteAsync(id);

        // Invalidate Redis cache
        await _cacheService.RemoveAsync($"listing:{id}");
        await _cacheService.RemoveByPrefixAsync("listings:catalog");
        await _cacheService.RemoveAsync("auctions:active");
    }

    public async Task<List<ListingListItemResponse>> GetMyListingsAsync(int userId)
    {
        var listings = await _listingRepository.GetByUserIdAsync(userId);
        return listings.Select(MapToListItem).ToList();
    }

    private static ListingListItemResponse MapToListItem(Listing listing)
    {
        return new ListingListItemResponse(
            listing.Id,
            listing.UserId,
            listing.Title,
            listing.Description,
            listing.Price,
            listing.IsAuction,
            listing.Status,
            listing.CreatedAt
        );
    }

    private static ListingDetailResponse MapToDetailResponse(Listing listing)
    {
        return new ListingDetailResponse(
            listing.Id,
            listing.UserId,
            listing.Title,
            listing.Description,
            listing.Price,
            listing.IsAuction,
            listing.Status,
            listing.CreatedAt,
            listing.UpdatedAt,
            listing.Images.Select(img => new ImageResponse(
                img.Id,
                img.FileName ?? "unknown",
                img.MimeType ?? "application/octet-stream",
                img.UploadedAt
            )).ToList()
        );
    }
}
