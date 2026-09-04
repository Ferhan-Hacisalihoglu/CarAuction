namespace CarAuction.Domain.Entities;

public class GroupMessage
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public int SenderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Group? Group { get; set; }
    public User? Sender { get; set; }
}
