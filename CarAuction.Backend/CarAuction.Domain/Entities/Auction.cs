namespace CarAuction.Domain.Entities;

public class Auction
{
    public int Id { get; set; }
    public int ListingId { get; set; }
    public decimal StartingPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal MinBidIncrement { get; set; }
    public int? WinnerUserId { get; set; }
    public string Status { get; set; } = "active";

    // Navigation properties
    public Listing? Listing { get; set; }
    public User? WinnerUser { get; set; }
}
