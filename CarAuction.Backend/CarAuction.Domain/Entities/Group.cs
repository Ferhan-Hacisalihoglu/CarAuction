namespace CarAuction.Domain.Entities;

public class Group
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
    public ICollection<GroupMessage> Messages { get; set; } = new List<GroupMessage>();
}
