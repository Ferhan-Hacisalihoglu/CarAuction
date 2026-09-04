using CarAuction.Application.DTOs.Groups;
using CarAuction.Domain.Entities;

namespace CarAuction.Application.Interfaces.Repositories;

public interface IGroupRepository
{
    Task<List<GroupResponse>> GetAllAsync();
    Task<List<GroupResponse>> GetByUserIdAsync(int userId);
    Task<int> CreateAsync(Group group);
    Task AddMemberAsync(int groupId, int userId);
    Task<Group?> GetByIdAsync(int id);
    Task<List<GroupMemberResponse>> GetMembersAsync(int groupId);
    Task<bool> IsMemberAsync(int groupId, int userId);
    Task JoinGroupAsync(int groupId, int userId);
    Task LeaveGroupAsync(int groupId, int userId);
    Task<List<GroupMessageResponse>> GetMessagesAsync(int groupId, int page, int limit);
    Task<GroupMessageResponse> SendMessageAsync(int groupId, int userId, string content);
}
