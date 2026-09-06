using CarAuction.Application.DTOs;
using CarAuction.Application.DTOs.Admin;
using CarAuction.Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarAuction.Presentation.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IListingRepository _listingRepository;
    private readonly IAuctionRepository _auctionRepository;
    private readonly IBidsRepository _bidsRepository;

    public AdminController(
        IUserRepository userRepository,
        IListingRepository listingRepository,
        IAuctionRepository auctionRepository,
        IBidsRepository bidsRepository)
    {
        _userRepository = userRepository;
        _listingRepository = listingRepository;
        _auctionRepository = auctionRepository;
        _bidsRepository = bidsRepository;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<AdminStatsResponse>>> GetStats()
    {
        var stats = new AdminStatsResponse(
            TotalUsers: await _userRepository.CountAsync(),
            TotalListings: await _listingRepository.CountAsync(),
            ActiveAuctions: await _auctionRepository.CountActiveAsync(),
            SoldListings: await _listingRepository.CountByStatusAsync("sold"),
            TotalBids24h: await _bidsRepository.CountRecentAsync(TimeSpan.FromHours(24)),
            NewUsers24h: await _userRepository.CountRecentAsync(TimeSpan.FromHours(24))
        );

        return Ok(ApiResponse<AdminStatsResponse>.SuccessResponse(stats));
    }

    [HttpGet("activity")]
    public async Task<ActionResult<ApiResponse<List<ActivityItemResponse>>>> GetActivity()
    {
        var activity = new List<ActivityItemResponse>();

        // Recent users
        var recentUsers = await _userRepository.GetRecentAsync(5);
        activity.AddRange(recentUsers.Select(u => new ActivityItemResponse(
            Type: "new_user",
            Detail: $"{u.FirstName} {u.LastName}",
            Timestamp: u.CreatedAt
        )));

        // Recent listings
        var recentListings = await _listingRepository.GetRecentAsync(5);
        activity.AddRange(recentListings.Select(l => new ActivityItemResponse(
            Type: "new_listing",
            Detail: l.Title,
            Timestamp: l.CreatedAt
        )));

        // Recent bids
        var recentBids = await _bidsRepository.GetRecentAsync(5);
        activity.AddRange(recentBids.Select(b => new ActivityItemResponse(
            Type: "new_bid",
            Detail: $"${b.Amount:N2} on {b.ListingTitle}",
            Timestamp: b.CreatedAt
        )));

        var sorted = activity.OrderByDescending(a => a.Timestamp).Take(20).ToList();

        return Ok(ApiResponse<List<ActivityItemResponse>>.SuccessResponse(sorted));
    }
}