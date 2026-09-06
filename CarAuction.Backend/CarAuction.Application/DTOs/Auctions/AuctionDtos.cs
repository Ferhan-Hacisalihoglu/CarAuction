namespace CarAuction.Application.DTOs.Auctions;

public record PlaceBidRequest(
    decimal Amount
);

public record AuctionListItemResponse(
    int Id,
    int ListingId,
    string Title,
    string? Description,
    int SellerId,
    decimal StartingPrice,
    decimal CurrentPrice,
    DateTime StartTime,
    DateTime EndTime,
    decimal MinBidIncrement,
    string Status,
    int? ImageId = null
);

public record AuctionDetailResponse(
    int Id,
    int ListingId,
    string Title,
    string? Description,
    int SellerId,
    decimal StartingPrice,
    decimal CurrentPrice,
    DateTime StartTime,
    DateTime EndTime,
    decimal MinBidIncrement,
    int? WinnerUserId,
    string Status
);

public record BidHistoryResponse(
    int Id,
    int ListingId,
    int UserId,
    string UserName,
    decimal Amount,
    DateTime CreatedAt
);
