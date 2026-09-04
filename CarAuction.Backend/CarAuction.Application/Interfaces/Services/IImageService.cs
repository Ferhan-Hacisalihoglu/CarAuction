using CarAuction.Application.DTOs.Images;

namespace CarAuction.Application.Interfaces.Services;

public interface IImageService
{
    Task<UploadImageResponse> UploadAsync(int listingId, int userId, Stream imageStream, string fileName, string mimeType);
    Task<(byte[] Data, string MimeType, string FileName)> GetByIdAsync(int id);
    Task DeleteAsync(int id, int userId, bool isAdmin);
}
