namespace CarAuction.Domain.Entities;

public class Message
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public int SenderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; }

    // Navigation properties
    public Conversation? Conversation { get; set; }
}
