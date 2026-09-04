using CarAuction.Application.DTOs;
using CarAuction.Application.DTOs.Auctions;
using CarAuction.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarAuction.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuctionsController : ControllerBase
{
    private readonly IAuctionService _auctionService;

    public AuctionsController(IAuctionService auctionService)
    {
        _auctionService = auctionService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AuctionListItemResponse>>>> GetActive()
    {
        var result = await _auctionService.GetActiveAuctionsAsync();
        return Ok(ApiResponse<List<AuctionListItemResponse>>.SuccessResponse(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<AuctionDetailResponse>>> GetById(int id)
    {
        var result = await _auctionService.GetByIdAsync(id);
        if (result == null)
        {
            return NotFound(ApiResponse<AuctionDetailResponse>.ErrorResponse("Auction not found"));
        }
        return Ok(ApiResponse<AuctionDetailResponse>.SuccessResponse(result));
    }

    [HttpGet("listing/{listingId:int}")]
    public async Task<ActionResult<ApiResponse<AuctionDetailResponse>>> GetByListingId(int listingId)
    {
        var result = await _auctionService.GetByListingIdAsync(listingId);
        if (result == null)
        {
            return NotFound(ApiResponse<AuctionDetailResponse>.ErrorResponse("Auction not found for this listing"));
        }
        return Ok(ApiResponse<AuctionDetailResponse>.SuccessResponse(result));
    }

    [HttpPost("{id:int}/bid")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<AuctionDetailResponse>>> PlaceBid(
        int id, [FromBody] PlaceBidRequest request)
    {
        var userId = GetUserId();
        var idempotencyKey = Request.Headers["X-Idempotency-Key"].FirstOrDefault();
        var result = await _auctionService.PlaceBidAsync(id, userId, request, idempotencyKey);
        return Ok(ApiResponse<AuctionDetailResponse>.SuccessResponse(result, "Bid placed successfully"));
    }

    [HttpGet("{id:int}/bids")]
    public async Task<ActionResult<ApiResponse<List<BidHistoryResponse>>>> GetBidHistory(int id)
    {
        var result = await _auctionService.GetBidHistoryAsync(id);
        return Ok(ApiResponse<List<BidHistoryResponse>>.SuccessResponse(result));
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user identity");
        }
        return userId;
    }
}
