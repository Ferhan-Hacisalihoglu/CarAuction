namespace CarAuction.Application.DTOs.Bids;

public record BidInfo(
    int Id,
    int ListingId,
    string ListingTitle,
    decimal Amount,
    DateTime CreatedAt
);