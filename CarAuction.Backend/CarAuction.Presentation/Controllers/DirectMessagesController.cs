using CarAuction.Application.DTOs;
using CarAuction.Application.DTOs.DirectMessages;
using CarAuction.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarAuction.Presentation.Controllers;

[ApiController]
[Route("api/dm")]
[Authorize]
public class DirectMessagesController : ControllerBase
{
    private readonly IDirectMessageService _dmService;

    public DirectMessagesController(IDirectMessageService dmService)
    {
        _dmService = dmService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ConversationResponse>>>> GetConversations()
    {
        var userId = GetUserId();
        var result = await _dmService.GetConversationsAsync(userId);
        return Ok(ApiResponse<List<ConversationResponse>>.SuccessResponse(result));
    }

    [HttpPost("start")]
    public async Task<ActionResult<ApiResponse<ConversationResponse>>> StartConversation([FromBody] StartConversationRequest request)
    {
        var userId = GetUserId();
        var result = await _dmService.StartConversationAsync(userId, request);
        return Ok(ApiResponse<ConversationResponse>.SuccessResponse(result, "Conversation started"));
    }

    [HttpGet("{conversationId:int}/messages")]
    public async Task<ActionResult<ApiResponse<List<MessageResponse>>>> GetMessages(
        int conversationId, [FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        var userId = GetUserId();
        var result = await _dmService.GetMessagesAsync(conversationId, userId, page, limit);
        return Ok(ApiResponse<List<MessageResponse>>.SuccessResponse(result));
    }

    [HttpPost("{conversationId:int}/messages")]
    public async Task<ActionResult<ApiResponse<MessageResponse>>> SendMessage(
        int conversationId, [FromBody] SendMessageRequest request)
    {
        var userId = GetUserId();
        var result = await _dmService.SendMessageAsync(conversationId, userId, request);
        return Ok(ApiResponse<MessageResponse>.SuccessResponse(result, "Message sent"));
    }

    [HttpPut("{conversationId:int}/read")]
    public async Task<ActionResult<ApiResponse<object>>> MarkAsRead(int conversationId)
    {
        var userId = GetUserId();
        await _dmService.MarkAsReadAsync(conversationId, userId);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Messages marked as read"));
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
