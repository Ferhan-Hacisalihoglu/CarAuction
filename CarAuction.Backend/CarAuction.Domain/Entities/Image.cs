namespace CarAuction.Domain.Entities;

public class Image
{
    public int Id { get; set; }
    public int ListingId { get; set; }
    public byte[] ImageData { get; set; } = Array.Empty<byte>();
    public string? FileName { get; set; }
    public string? MimeType { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Listing? Listing { get; set; }
}
