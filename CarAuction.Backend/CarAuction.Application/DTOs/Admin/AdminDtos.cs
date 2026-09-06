namespace CarAuction.Application.DTOs.Admin;

public record AdminStatsResponse(
    long TotalUsers,
    long TotalListings,
    long ActiveAuctions,
    long SoldListings,
    long TotalBids24h,
    long NewUsers24h
);

public record ActivityItemResponse(
    string Type,
    string Detail,
    DateTime Timestamp
);