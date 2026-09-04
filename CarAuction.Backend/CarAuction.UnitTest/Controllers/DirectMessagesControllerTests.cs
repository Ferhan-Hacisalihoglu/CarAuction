using CarAuction.Application.DTOs;
using CarAuction.Application.DTOs.DirectMessages;
using CarAuction.Application.Interfaces.Services;
using CarAuction.Presentation.Controllers;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CarAuction.UnitTest.Controllers;

public class DirectMessagesControllerTests
{
    private readonly IDirectMessageService _service;
    private readonly DirectMessagesController _controller;

    public DirectMessagesControllerTests()
    {
        _service = A.Fake<IDirectMessageService>();
        _controller = new DirectMessagesController(_service);
    }

    [Fact]
    public async Task GetConversations_Returns200Ok()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        var expectedConversations = new List<ConversationResponse>
        {
            new(1, 1, 2, "Jane", "Smith", "Hello", false, DateTime.UtcNow),
            new(2, 1, 3, "Bob", "Johnson", null, null, DateTime.UtcNow)
        };

        A.CallTo(() => _service.GetConversationsAsync(1))
            .Returns(expectedConversations);

        // Act
        var result = await _controller.GetConversations();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<List<ConversationResponse>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(expectedConversations);
    }

    [Fact]
    public async Task StartConversation_Valid_Returns200Ok()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        var request = new StartConversationRequest(2);
        var expectedResponse = new ConversationResponse(1, 1, 2, "Jane", "Smith", null, null, DateTime.UtcNow);

        A.CallTo(() => _service.StartConversationAsync(1, request))
            .Returns(expectedResponse);

        // Act
        var result = await _controller.StartConversation(request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<ConversationResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task StartConversation_Self_ThrowsInvalidOperationException()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        var request = new StartConversationRequest(1);

        A.CallTo(() => _service.StartConversationAsync(1, request))
            .Throws(new InvalidOperationException("You cannot start a conversation with yourself"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.StartConversation(request));
    }

    [Fact]
    public async Task GetMessages_Participant_Returns200Ok()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        var expectedMessages = new List<MessageResponse>
        {
            new(1, 1, 1, "Hello", DateTime.UtcNow.AddMinutes(-10), true),
            new(2, 1, 2, "Hi there!", DateTime.UtcNow.AddMinutes(-5), false)
        };

        A.CallTo(() => _service.GetMessagesAsync(1, 1, 1, 20))
            .Returns(expectedMessages);

        // Act
        var result = await _controller.GetMessages(1, 1, 20);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<List<MessageResponse>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(expectedMessages);
    }

    [Fact]
    public async Task GetMessages_NotParticipant_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "3") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        A.CallTo(() => _service.GetMessagesAsync(1, 3, 1, 20))
            .Throws(new UnauthorizedAccessException("You are not a participant of this conversation"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.GetMessages(1, 1, 20));
    }

    [Fact]
    public async Task SendMessage_Participant_Returns200Ok()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        var request = new SendMessageRequest("Hello!");
        var expectedResponse = new MessageResponse(1, 1, 1, "Hello!", DateTime.UtcNow, false);

        A.CallTo(() => _service.SendMessageAsync(1, 1, request))
            .Returns(expectedResponse);

        // Act
        var result = await _controller.SendMessage(1, request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<MessageResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.Content.Should().Be("Hello!");
    }

    [Fact]
    public async Task SendMessage_NotParticipant_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "3") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        var request = new SendMessageRequest("Hello!");

        A.CallTo(() => _service.SendMessageAsync(1, 3, request))
            .Throws(new UnauthorizedAccessException("You are not a participant of this conversation"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.SendMessage(1, request));
    }

    [Fact]
    public async Task MarkAsRead_Participant_Returns200Ok()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        A.CallTo(() => _service.MarkAsReadAsync(1, 1))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.MarkAsRead(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Success.Should().BeTrue();
    }

    [Fact]
    public async Task MarkAsRead_NotParticipant_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "3") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        A.CallTo(() => _service.MarkAsReadAsync(1, 3))
            .Throws(new UnauthorizedAccessException("You are not a participant of this conversation"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.MarkAsRead(1));
    }

    [Fact]
    public async Task GetUserId_InvalidClaim_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "invalid") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.GetConversations());
    }
}
