using CarAuction.Application.DTOs.Groups;
using CarAuction.Application.Interfaces.Repositories;
using CarAuction.Application.Interfaces.Services;

namespace CarAuction.Application.Services;

public class GroupService : IGroupService
{
    private readonly IGroupRepository _groupRepository;
    private readonly INotificationService _notificationService;

    public GroupService(IGroupRepository groupRepository, INotificationService notificationService)
    {
        _groupRepository = groupRepository;
        _notificationService = notificationService;
    }

    public async Task<List<GroupResponse>> GetAllAsync()
    {
        return await _groupRepository.GetAllAsync();
    }

    public async Task<List<GroupResponse>> GetMyGroupsAsync(int userId)
    {
        return await _groupRepository.GetByUserIdAsync(userId);
    }

    public async Task<GroupResponse> CreateAsync(int userId, CreateGroupRequest request)
    {
        var group = new Domain.Entities.Group
        {
            Name = request.Name,
            Description = request.Description,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        var id = await _groupRepository.CreateAsync(group);

        return new GroupResponse(id, request.Name, request.Description, userId, group.CreatedAt);
    }

    public async Task<GroupDetailResponse> GetByIdAsync(int id)
    {
        var group = await _groupRepository.GetByIdAsync(id);
        if (group == null)
        {
            throw new KeyNotFoundException($"Group with ID {id} not found");
        }

        var members = await _groupRepository.GetMembersAsync(id);

        return new GroupDetailResponse(
            group.Id,
            group.Name,
            group.Description,
            group.CreatedBy ?? 0,
            group.CreatedAt,
            members
        );
    }

    public async Task JoinGroupAsync(int groupId, int userId)
    {
        var group = await _groupRepository.GetByIdAsync(groupId);
        if (group == null)
        {
            throw new KeyNotFoundException($"Group with ID {groupId} not found");
        }

        if (await _groupRepository.IsMemberAsync(groupId, userId))
        {
            throw new InvalidOperationException("You are already a member of this group");
        }

        await _groupRepository.JoinGroupAsync(groupId, userId);
    }

    public async Task LeaveGroupAsync(int groupId, int userId)
    {
        var group = await _groupRepository.GetByIdAsync(groupId);
        if (group == null)
        {
            throw new KeyNotFoundException($"Group with ID {groupId} not found");
        }

        if (!await _groupRepository.IsMemberAsync(groupId, userId))
        {
            throw new InvalidOperationException("You are not a member of this group");
        }

        await _groupRepository.LeaveGroupAsync(groupId, userId);
    }

    public async Task<List<GroupMessageResponse>> GetMessagesAsync(int groupId, int userId, int page, int limit)
    {
        // Check membership
        if (!await _groupRepository.IsMemberAsync(groupId, userId))
        {
            throw new UnauthorizedAccessException("You are not a member of this group");
        }

        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 100);

        return await _groupRepository.GetMessagesAsync(groupId, page, limit);
    }

    public async Task<GroupMessageResponse> SendMessageAsync(int groupId, int userId, SendMessageRequest request)
    {
        // Check membership
        if (!await _groupRepository.IsMemberAsync(groupId, userId))
        {
            throw new UnauthorizedAccessException("You are not a member of this group");
        }

        var message = await _groupRepository.SendMessageAsync(groupId, userId, request.Content);

        // Broadcast to group via ChatHub
        await _notificationService.NotifyGroupMessageAsync(groupId, message);

        return message;
    }
}
