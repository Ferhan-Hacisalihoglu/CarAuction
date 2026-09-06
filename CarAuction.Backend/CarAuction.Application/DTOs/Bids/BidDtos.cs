namespace CarAuction.Application.DTOs.Bids;

public record MakeOfferRequest(
    decimal Amount
);

public record OfferResponse(
    int Id,
    int ListingId,
    string ListingTitle,
    int UserId,
    string UserName,
    decimal Amount,
    DateTime CreatedAt
);

public record OfferWithStatusResponse(
    int Id,
    int ListingId,
    string ListingTitle,
    int UserId,
    string UserName,
    decimal Amount,
    string Status,
    DateTime CreatedAt
);

public record MyBidResponse(
    int Id,
    int ListingId,
    string ListingTitle,
    decimal ListingPrice,
    string ListingStatus,
    decimal Amount,
    DateTime CreatedAt
);

public record OfferStatusUpdateRequest(
    string Status
);
