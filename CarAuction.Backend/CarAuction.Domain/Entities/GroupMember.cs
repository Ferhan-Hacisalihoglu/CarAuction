namespace CarAuction.Domain.Entities;

public class GroupMember
{
    public int GroupId { get; set; }
    public int UserId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Group? Group { get; set; }
    public User? User { get; set; }
}
