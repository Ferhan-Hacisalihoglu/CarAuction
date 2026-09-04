namespace CarAuction.Application.DTOs.Listings;

public record CreateListingRequest(
    string Title,
    string? Description,
    decimal Price,
    bool IsAuction,
    decimal? StartingPrice,
    DateTime? StartTime,
    DateTime? EndTime,
    decimal? MinBidIncrement
);

public record UpdateListingRequest(
    string Title,
    string? Description,
    decimal Price
);

public record ListingFilters(
    string? Search,
    decimal? MinPrice,
    decimal? MaxPrice,
    string? Status,
    bool? IsAuction,
    int Page,
    int Limit
);

public record ListingListItemResponse(
    int Id,
    int UserId,
    string Title,
    string? Description,
    decimal Price,
    bool IsAuction,
    string Status,
    DateTime CreatedAt
);

public record ListingDetailResponse(
    int Id,
    int UserId,
    string Title,
    string? Description,
    decimal Price,
    bool IsAuction,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<ImageResponse> Images
);

public record ImageResponse(
    int Id,
    string FileName,
    string MimeType,
    DateTime UploadedAt
);
