using CarAuction.Application.DTOs.Auctions;
using CarAuction.Application.Interfaces.Caching;
using CarAuction.Application.Interfaces.Repositories;
using CarAuction.Application.Interfaces.Services;

namespace CarAuction.Application.Services;

public class AuctionService : IAuctionService
{
    private readonly IAuctionRepository _auctionRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICacheService _cacheService;
    private readonly INotificationService _notificationService;

    public AuctionService(
        IAuctionRepository auctionRepository,
        IUserRepository userRepository,
        ICacheService cacheService,
        INotificationService notificationService)
    {
        _auctionRepository = auctionRepository;
        _userRepository = userRepository;
        _cacheService = cacheService;
        _notificationService = notificationService;
    }

    public async Task<List<AuctionListItemResponse>> GetActiveAuctionsAsync()
    {
        return await _auctionRepository.GetActiveAuctionsAsync();
    }

    public async Task<AuctionDetailResponse?> GetByIdAsync(int id)
    {
        var auction = await _auctionRepository.GetByIdAsync(id);
        if (auction == null)
        {
            return null;
        }

        // Check Redis live cache for sub-millisecond real-time price & timer
        var cachedState = await _cacheService.GetAsync<LiveAuctionState>($"auction:{id}:state");
        if (cachedState != null)
        {
            return auction with
            {
                CurrentPrice = cachedState.CurrentPrice,
                WinnerUserId = cachedState.WinnerUserId,
                EndTime = cachedState.EndTime,
                Status = cachedState.Status
            };
        }

        // Cache the live state in Redis
        await _cacheService.SetAsync($"auction:{id}:state",
            new LiveAuctionState(auction.CurrentPrice, auction.WinnerUserId, auction.EndTime, auction.Status),
            TimeSpan.FromDays(1));

        return auction;
    }

    public async Task<AuctionDetailResponse?> GetByListingIdAsync(int listingId)
    {
        return await _auctionRepository.GetByListingIdAsync(listingId);
    }

    public async Task<AuctionDetailResponse> PlaceBidAsync(int auctionId, int userId, PlaceBidRequest request, string? idempotencyKey)
    {
        // 1. Idempotency Check (Redis first, then DB)
        if (!string.IsNullOrEmpty(idempotencyKey))
        {
            var cachedResponse = await _cacheService.GetAsync<AuctionDetailResponse>($"idempotency:bid:{idempotencyKey}");
            if (cachedResponse != null)
            {
                return cachedResponse;
            }

            if (await _auctionRepository.IdempotencyKeyExistsAsync(idempotencyKey))
            {
                return await GetByIdAsync(auctionId)
                    ?? throw new InvalidOperationException("Auction not found");
            }
        }

        // 2. Validate current auction state
        var auction = await _auctionRepository.GetByIdAsync(auctionId);
        if (auction == null)
        {
            throw new KeyNotFoundException($"Auction with ID {auctionId} not found");
        }

        if (auction.Status != "active")
        {
            throw new InvalidOperationException("Auction is not active");
        }

        if (DateTime.UtcNow < auction.StartTime)
        {
            throw new InvalidOperationException("Auction has not started yet");
        }

        if (DateTime.UtcNow >= auction.EndTime)
        {
            throw new InvalidOperationException("Auction has ended");
        }

        var minimumBid = auction.CurrentPrice + auction.MinBidIncrement;
        if (request.Amount < minimumBid)
        {
            throw new InvalidOperationException($"Minimum bid is {minimumBid}");
        }

        // 3. Execute ADO.NET pessimistic transaction with row lock
        var updatedAuction = await _auctionRepository.PlaceBidWithTransactionAsync(auctionId, userId, request.Amount, idempotencyKey);

        // 4. Update Redis live auction cache
        await _cacheService.SetAsync($"auction:{auctionId}:state",
            new LiveAuctionState(updatedAuction.CurrentPrice, updatedAuction.WinnerUserId, updatedAuction.EndTime, updatedAuction.Status),
            TimeSpan.FromDays(1));

        // 5. Cache Idempotency key if provided (24-hour expiration)
        if (!string.IsNullOrEmpty(idempotencyKey))
        {
            await _cacheService.SetAsync($"idempotency:bid:{idempotencyKey}", updatedAuction, TimeSpan.FromHours(24));
        }

        // 6. Broadcast ReceiveNewBid via SignalR AuctionHub
        var user = await _userRepository.GetByIdAsync(userId);
        var bidderName = user != null ? $"{user.FirstName} {user.LastName}" : $"User #{userId}";

        await _notificationService.NotifyNewBidAsync(auctionId, new AuctionHubDto(
            auctionId,
            request.Amount,
            userId,
            bidderName,
            DateTime.UtcNow
        ));

        // 7. Check if anti-sniping extended the auction and broadcast AuctionTimeExtended
        if (updatedAuction.EndTime > auction.EndTime)
        {
            var extendedSeconds = (int)(updatedAuction.EndTime - auction.EndTime).TotalSeconds;
            await _notificationService.NotifyAuctionTimeExtendedAsync(auctionId, new AuctionExtendedDto(
                auctionId,
                updatedAuction.EndTime,
                extendedSeconds
            ));
        }

        return updatedAuction;
    }

    public async Task<List<BidHistoryResponse>> GetBidHistoryAsync(int auctionId)
    {
        var auction = await _auctionRepository.GetByIdAsync(auctionId);
        if (auction == null)
        {
            throw new KeyNotFoundException($"Auction with ID {auctionId} not found");
        }

        return await _auctionRepository.GetBidHistoryAsync(auctionId);
    }
}
