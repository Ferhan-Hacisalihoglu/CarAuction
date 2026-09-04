using CarAuction.Application.DTOs.Groups;

namespace CarAuction.Application.Interfaces.Services;

public interface IGroupService
{
    Task<List<GroupResponse>> GetAllAsync();
    Task<List<GroupResponse>> GetMyGroupsAsync(int userId);
    Task<GroupResponse> CreateAsync(int userId, CreateGroupRequest request);
    Task<GroupDetailResponse> GetByIdAsync(int id);
    Task JoinGroupAsync(int groupId, int userId);
    Task LeaveGroupAsync(int groupId, int userId);
    Task<List<GroupMessageResponse>> GetMessagesAsync(int groupId, int userId, int page, int limit);
    Task<GroupMessageResponse> SendMessageAsync(int groupId, int userId, SendMessageRequest request);
}
