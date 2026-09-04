namespace CarAuction.Application.DTOs.Auctions;

public record AuctionHubDto(
    int AuctionId,
    decimal Amount,
    int BidderId,
    string BidderName,
    DateTime Timestamp
);

public record AuctionResultDto(
    int AuctionId,
    int? WinnerUserId,
    string? WinnerName,
    decimal WinningPrice,
    string Status
);

public record AuctionExtendedDto(
    int AuctionId,
    DateTime NewEndTime,
    int ExtendedSeconds
);

public record LiveAuctionState(
    decimal CurrentPrice,
    int? WinnerUserId,
    DateTime EndTime,
    string Status
);
