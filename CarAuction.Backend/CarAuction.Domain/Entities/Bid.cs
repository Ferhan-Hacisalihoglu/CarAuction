namespace CarAuction.Domain.Entities;

public class Bid
{
    public int Id { get; set; }
    public int ListingId { get; set; }
    public int UserId { get; set; }
    public decimal Amount { get; set; }
    public string? IdempotencyKey { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Listing? Listing { get; set; }
    public User? User { get; set; }
}
