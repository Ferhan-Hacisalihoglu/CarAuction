namespace CarAuction.Application.DTOs.Images;

public record UploadImageResponse(
    int Id,
    string FileName,
    string MimeType,
    long FileSize,
    DateTime UploadedAt
);
