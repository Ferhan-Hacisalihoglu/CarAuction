using CarAuction.Application.DTOs;
using CarAuction.Application.DTOs.Listings;
using CarAuction.Application.DTOs.Users;
using CarAuction.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarAuction.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ListingsController : ControllerBase
{
    private readonly IListingService _listingService;

    public ListingsController(IListingService listingService)
    {
        _listingService = listingService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<ListingListItemResponse>>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? status,
        [FromQuery] bool? isAuction,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10)
    {
        var filters = new ListingFilters(search, minPrice, maxPrice, status, isAuction, page, limit);
        var result = await _listingService.GetAllAsync(filters);
        return Ok(ApiResponse<PaginatedResponse<ListingListItemResponse>>.SuccessResponse(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ListingDetailResponse>>> GetById(int id)
    {
        var result = await _listingService.GetByIdAsync(id);
        if (result == null)
        {
            return NotFound(ApiResponse<ListingDetailResponse>.ErrorResponse("Listing not found"));
        }
        return Ok(ApiResponse<ListingDetailResponse>.SuccessResponse(result));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ListingDetailResponse>>> Create([FromBody] CreateListingRequest request)
    {
        var userId = GetUserId();
        var result = await _listingService.CreateAsync(userId, request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ListingDetailResponse>.SuccessResponse(result, "Listing created successfully"));
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ListingDetailResponse>>> Update(int id, [FromBody] UpdateListingRequest request)
    {
        var userId = GetUserId();
        var result = await _listingService.UpdateAsync(id, userId, request);
        return Ok(ApiResponse<ListingDetailResponse>.SuccessResponse(result, "Listing updated successfully"));
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var userId = GetUserId();
        await _listingService.DeleteAsync(id, userId);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Listing cancelled successfully"));
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<List<ListingListItemResponse>>>> GetMyListings()
    {
        var userId = GetUserId();
        var result = await _listingService.GetMyListingsAsync(userId);
        return Ok(ApiResponse<List<ListingListItemResponse>>.SuccessResponse(result));
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
