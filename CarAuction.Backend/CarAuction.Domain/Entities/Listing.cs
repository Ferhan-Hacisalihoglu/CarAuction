namespace CarAuction.Domain.Entities;

public class Listing
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsAuction { get; set; }
    public string Status { get; set; } = "active";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public Auction? Auction { get; set; }
    public ICollection<Image> Images { get; set; } = new List<Image>();
}
