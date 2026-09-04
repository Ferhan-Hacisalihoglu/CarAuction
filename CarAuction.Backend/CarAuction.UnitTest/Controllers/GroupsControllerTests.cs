using CarAuction.Application.DTOs;
using CarAuction.Application.DTOs.Groups;
using CarAuction.Application.Interfaces.Services;
using CarAuction.Presentation.Controllers;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CarAuction.UnitTest.Controllers;

public class GroupsControllerTests
{
    private readonly IGroupService _groupService;
    private readonly GroupsController _controller;

    public GroupsControllerTests()
    {
        _groupService = A.Fake<IGroupService>();
        _controller = new GroupsController(_groupService);
    }

    [Fact]
    public async Task GetAll_Returns200Ok()
    {
        // Arrange
        var expectedGroups = new List<GroupResponse>
        {
            new(1, "Car Enthusiasts", "A group for car lovers", 1, DateTime.UtcNow),
            new(2, "Tesla Owners", "Tesla owners community", 2, DateTime.UtcNow)
        };

        A.CallTo(() => _groupService.GetAllAsync())
            .Returns(expectedGroups);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<List<GroupResponse>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(expectedGroups);
    }

    [Fact]
    public async Task GetMyGroups_Returns200Ok()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        var expectedGroups = new List<GroupResponse>
        {
            new(1, "Car Enthusiasts", "A group for car lovers", 1, DateTime.UtcNow)
        };

        A.CallTo(() => _groupService.GetMyGroupsAsync(1))
            .Returns(expectedGroups);

        // Act
        var result = await _controller.GetMyGroups();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<List<GroupResponse>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(expectedGroups);
    }

    [Fact]
    public async Task Create_ValidRequest_Returns201Created()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        var request = new CreateGroupRequest("New Group", "Description");
        var expectedResponse = new GroupResponse(1, "New Group", "Description", 1, DateTime.UtcNow);

        A.CallTo(() => _groupService.CreateAsync(1, request))
            .Returns(expectedResponse);

        // Act
        var result = await _controller.Create(request);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);

        var response = createdResult.Value.Should().BeOfType<ApiResponse<GroupResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetById_Existing_Returns200Ok()
    {
        // Arrange
        var expectedGroup = new GroupDetailResponse(
            1, "Car Enthusiasts", "A group for car lovers", 1, DateTime.UtcNow,
            new List<GroupMemberResponse>
            {
                new(1, "John", "Doe", DateTime.UtcNow),
                new(2, "Jane", "Smith", DateTime.UtcNow)
            });

        A.CallTo(() => _groupService.GetByIdAsync(1))
            .Returns(expectedGroup);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<GroupDetailResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(expectedGroup);
    }

    [Fact]
    public async Task GetById_NonExisting_ThrowsKeyNotFoundException()
    {
        // Arrange
        A.CallTo(() => _groupService.GetByIdAsync(999))
            .Throws(new KeyNotFoundException("Group with ID 999 not found"));

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.GetById(999));
    }

    [Fact]
    public async Task Join_Authenticated_Returns200Ok()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "2") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        A.CallTo(() => _groupService.JoinGroupAsync(1, 2))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Join(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Join_AlreadyMember_ThrowsInvalidOperationException()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        A.CallTo(() => _groupService.JoinGroupAsync(1, 1))
            .Throws(new InvalidOperationException("You are already a member of this group"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Join(1));
    }

    [Fact]
    public async Task Leave_Authenticated_Returns200Ok()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        A.CallTo(() => _groupService.LeaveGroupAsync(1, 1))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Leave(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Leave_NotMember_ThrowsInvalidOperationException()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "2") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        A.CallTo(() => _groupService.LeaveGroupAsync(1, 2))
            .Throws(new InvalidOperationException("You are not a member of this group"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Leave(1));
    }

    [Fact]
    public async Task GetMessages_Member_Returns200Ok()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        var expectedMessages = new List<GroupMessageResponse>
        {
            new(1, 1, 1, "John Doe", "Hello everyone!", DateTime.UtcNow.AddMinutes(-10)),
            new(2, 1, 2, "Jane Smith", "Hi John!", DateTime.UtcNow.AddMinutes(-5))
        };

        A.CallTo(() => _groupService.GetMessagesAsync(1, 1, 1, 20))
            .Returns(expectedMessages);

        // Act
        var result = await _controller.GetMessages(1, 1, 20);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<List<GroupMessageResponse>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(expectedMessages);
    }

    [Fact]
    public async Task GetMessages_NotMember_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "2") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        A.CallTo(() => _groupService.GetMessagesAsync(1, 2, 1, 20))
            .Throws(new UnauthorizedAccessException("You are not a member of this group"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.GetMessages(1, 1, 20));
    }

    [Fact]
    public async Task SendMessage_Member_Returns200Ok()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        var request = new SendMessageRequest("Hello!");
        var expectedResponse = new GroupMessageResponse(1, 1, 1, "John Doe", "Hello!", DateTime.UtcNow);

        A.CallTo(() => _groupService.SendMessageAsync(1, 1, request))
            .Returns(expectedResponse);

        // Act
        var result = await _controller.SendMessage(1, request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<GroupMessageResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.Content.Should().Be("Hello!");
    }

    [Fact]
    public async Task SendMessage_NotMember_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "2") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        var request = new SendMessageRequest("Hello!");

        A.CallTo(() => _groupService.SendMessageAsync(1, 2, request))
            .Throws(new UnauthorizedAccessException("You are not a member of this group"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.SendMessage(1, request));
    }

    [Fact]
    public async Task GetUserId_InvalidClaim_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "invalid") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        // Act & Assert - GetMyGroups calls GetUserId internally
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.GetMyGroups());
    }
}
