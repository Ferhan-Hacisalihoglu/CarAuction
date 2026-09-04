using CarAuction.Application.DTOs;
using CarAuction.Application.DTOs.Images;
using CarAuction.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarAuction.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImagesController : ControllerBase
{
    private readonly IImageService _imageService;

    public ImagesController(IImageService imageService)
    {
        _imageService = imageService;
    }

    [HttpPost("/api/listings/{listingId:int}/images")]
    [Authorize]
    [RequestSizeLimit(10_485_760)] // 10MB
    public async Task<ActionResult<ApiResponse<UploadImageResponse>>> Upload(
        int listingId, IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(ApiResponse<UploadImageResponse>.ErrorResponse("No file uploaded"));
        }

        var userId = GetUserId();

        using var stream = file.OpenReadStream();
        var result = await _imageService.UploadAsync(listingId, userId, stream, file.FileName, file.ContentType);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<UploadImageResponse>.SuccessResponse(result, "Image uploaded successfully"));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var (data, mimeType, fileName) = await _imageService.GetByIdAsync(id);
        return File(data, mimeType, fileName);
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var userId = GetUserId();
        var isAdmin = User.IsInRole("Admin");

        await _imageService.DeleteAsync(id, userId, isAdmin);

        return Ok(ApiResponse<object>.SuccessResponse(null!, "Image deleted successfully"));
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
