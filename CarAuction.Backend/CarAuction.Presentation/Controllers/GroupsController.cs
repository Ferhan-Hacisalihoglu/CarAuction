using CarAuction.Application.DTOs;
using CarAuction.Application.DTOs.Groups;
using CarAuction.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarAuction.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GroupsController : ControllerBase
{
    private readonly IGroupService _groupService;

    public GroupsController(IGroupService groupService)
    {
        _groupService = groupService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<GroupResponse>>>> GetAll()
    {
        var result = await _groupService.GetAllAsync();
        return Ok(ApiResponse<List<GroupResponse>>.SuccessResponse(result));
    }

    [HttpGet("my")]
    public async Task<ActionResult<ApiResponse<List<GroupResponse>>>> GetMyGroups()
    {
        var userId = GetUserId();
        var result = await _groupService.GetMyGroupsAsync(userId);
        return Ok(ApiResponse<List<GroupResponse>>.SuccessResponse(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<GroupResponse>>> Create([FromBody] CreateGroupRequest request)
    {
        var userId = GetUserId();
        var result = await _groupService.CreateAsync(userId, request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<GroupResponse>.SuccessResponse(result, "Group created successfully"));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<GroupDetailResponse>>> GetById(int id)
    {
        var result = await _groupService.GetByIdAsync(id);
        return Ok(ApiResponse<GroupDetailResponse>.SuccessResponse(result));
    }

    [HttpPost("{id:int}/join")]
    public async Task<ActionResult<ApiResponse<object>>> Join(int id)
    {
        var userId = GetUserId();
        await _groupService.JoinGroupAsync(id, userId);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Joined group successfully"));
    }

    [HttpPost("{id:int}/leave")]
    public async Task<ActionResult<ApiResponse<object>>> Leave(int id)
    {
        var userId = GetUserId();
        await _groupService.LeaveGroupAsync(id, userId);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Left group successfully"));
    }

    [HttpGet("{id:int}/messages")]
    public async Task<ActionResult<ApiResponse<List<GroupMessageResponse>>>> GetMessages(
        int id, [FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        var userId = GetUserId();
        var result = await _groupService.GetMessagesAsync(id, userId, page, limit);
        return Ok(ApiResponse<List<GroupMessageResponse>>.SuccessResponse(result));
    }

    [HttpPost("{id:int}/messages")]
    public async Task<ActionResult<ApiResponse<GroupMessageResponse>>> SendMessage(
        int id, [FromBody] SendMessageRequest request)
    {
        var userId = GetUserId();
        var result = await _groupService.SendMessageAsync(id, userId, request);
        return Ok(ApiResponse<GroupMessageResponse>.SuccessResponse(result, "Message sent successfully"));
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user identity");
        }
        return userId;
    }
}
