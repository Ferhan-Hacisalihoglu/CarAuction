using CarAuction.Application.DTOs.Images;
using CarAuction.Application.Interfaces.Repositories;
using CarAuction.Application.Interfaces.Services;

namespace CarAuction.Application.Services;

public class ImageService : IImageService
{
    private readonly IImageRepository _imageRepository;
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public ImageService(IImageRepository imageRepository)
    {
        _imageRepository = imageRepository;
    }

    public async Task<UploadImageResponse> UploadAsync(int listingId, int userId, Stream imageStream, string fileName, string mimeType)
    {
        // Check if listing exists and user is owner or admin
        var listing = await _imageRepository.GetListingByIdAsync(listingId);
        if (listing == null)
        {
            throw new KeyNotFoundException($"Listing with ID {listingId} not found");
        }

        if (listing.UserId != userId)
        {
            // Check if user is admin
            var isAdmin = await _imageRepository.IsUserAdminAsync(userId);
            if (!isAdmin)
            {
                throw new UnauthorizedAccessException("You are not the owner of this listing");
            }
        }

        // Read stream to byte array
        using var memoryStream = new MemoryStream();
        await imageStream.CopyToAsync(memoryStream);
        var imageData = memoryStream.ToArray();

        // Validate file size
        if (imageData.Length > MaxFileSize)
        {
            throw new InvalidOperationException($"File size exceeds maximum allowed size of 10MB");
        }

        // Validate mime type
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(mimeType.ToLower()))
        {
            throw new InvalidOperationException("Invalid image format. Allowed formats: JPEG, PNG, GIF, WebP");
        }

        var image = new Domain.Entities.Image
        {
            ListingId = listingId,
            ImageData = imageData,
            FileName = fileName,
            MimeType = mimeType,
            UploadedAt = DateTime.UtcNow
        };

        var id = await _imageRepository.CreateAsync(image);

        return new UploadImageResponse(
            id,
            fileName,
            mimeType,
            imageData.Length,
            image.UploadedAt
        );
    }

    public async Task<(byte[] Data, string MimeType, string FileName)> GetByIdAsync(int id)
    {
        var image = await _imageRepository.GetByIdAsync(id);
        if (image == null)
        {
            throw new KeyNotFoundException($"Image with ID {id} not found");
        }

        return (image.ImageData, image.MimeType ?? "application/octet-stream", image.FileName ?? "unknown");
    }

    public async Task DeleteAsync(int id, int userId, bool isAdmin)
    {
        var image = await _imageRepository.GetByIdAsync(id);
        if (image == null)
        {
            throw new KeyNotFoundException($"Image with ID {id} not found");
        }

        // Check if user is owner or admin
        var listing = await _imageRepository.GetListingByIdAsync(image.ListingId);
        if (listing == null)
        {
            throw new KeyNotFoundException($"Listing with ID {image.ListingId} not found");
        }

        if (listing.UserId != userId && !isAdmin)
        {
            throw new UnauthorizedAccessException("You are not the owner of this listing");
        }

        await _imageRepository.DeleteAsync(id);
    }
}
