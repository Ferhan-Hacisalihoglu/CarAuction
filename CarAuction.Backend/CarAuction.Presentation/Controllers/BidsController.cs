using CarAuction.Application.DTOs;
using CarAuction.Application.DTOs.Bids;
using CarAuction.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarAuction.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BidsController : ControllerBase
{
    private readonly IBidsService _bidsService;

    public BidsController(IBidsService bidsService)
    {
        _bidsService = bidsService;
    }

    [HttpPost("/api/listings/{listingId:int}/offers")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<OfferResponse>>> MakeOffer(
        int listingId, [FromBody] MakeOfferRequest request)
    {
        var userId = GetUserId();
        var result = await _bidsService.MakeOfferAsync(listingId, userId, request);
        return CreatedAtAction(nameof(GetOffers), new { listingId }, ApiResponse<OfferResponse>.SuccessResponse(result, "Offer made successfully"));
    }

    [HttpGet("/api/listings/{listingId:int}/offers")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<List<OfferResponse>>>> GetOffers(int listingId)
    {
        var userId = GetUserId();
        var result = await _bidsService.GetOffersAsync(listingId, userId);
        return Ok(ApiResponse<List<OfferResponse>>.SuccessResponse(result));
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<List<MyBidResponse>>>> GetMyBids()
    {
        var userId = GetUserId();
        var result = await _bidsService.GetMyBidsAsync(userId);
        return Ok(ApiResponse<List<MyBidResponse>>.SuccessResponse(result));
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
