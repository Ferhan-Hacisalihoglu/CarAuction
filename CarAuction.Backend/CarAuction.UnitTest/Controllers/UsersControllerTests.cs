using CarAuction.Application.DTOs;
using CarAuction.Application.DTOs.Users;
using CarAuction.Application.Interfaces.Services;
using CarAuction.Presentation.Controllers;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CarAuction.UnitTest.Controllers;

public class UsersControllerTests
{
    private readonly IUserService _userService;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _userService = A.Fake<IUserService>();
        _controller = new UsersController(_userService);
    }

    [Fact]
    public async Task GetAll_AdminUser_Returns200Ok()
    {
        // Arrange
        var expectedResponse = new PaginatedResponse<UserListResponse>(
            new List<UserListResponse>
            {
                new(1, "John", "Doe", "john@example.com", "User", true, DateTime.UtcNow),
                new(2, "Jane", "Smith", "jane@example.com", "Admin", true, DateTime.UtcNow)
            },
            2, 1, 10, 1
        );

        A.CallTo(() => _userService.GetAllAsync(1, 10, null))
            .Returns(expectedResponse);

        // Act
        var result = await _controller.GetAll(1, 10);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<PaginatedResponse<UserListResponse>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetById_ExistingUser_Returns200Ok()
    {
        // Arrange
        var expectedUser = new UserListResponse(1, "John", "Doe", "john@example.com", "User", true, DateTime.UtcNow);

        A.CallTo(() => _userService.GetByIdAsync(1))
            .Returns(expectedUser);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<UserListResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(expectedUser);
    }

    [Fact]
    public async Task GetById_NonExistingUser_Returns404NotFound()
    {
        // Arrange
        A.CallTo(() => _userService.GetByIdAsync(999))
            .Returns((UserListResponse?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);

        var response = notFoundResult.Value.Should().BeOfType<ApiResponse<UserListResponse>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("User not found");
    }

    [Fact]
    public async Task UpdateProfile_AuthenticatedUser_Returns200Ok()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        var request = new UpdateProfileRequest("John", "Updated");
        var expectedResponse = new UserListResponse(1, "John", "Updated", "john@example.com", "User", true, DateTime.UtcNow);

        A.CallTo(() => _userService.UpdateProfileAsync(1, request))
            .Returns(expectedResponse);

        // Act
        var result = await _controller.UpdateProfile(request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<UserListResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.LastName.Should().Be("Updated");
    }

    [Fact]
    public async Task UpdateStatus_AdminUser_Returns200Ok()
    {
        // Arrange
        var request = new UpdateUserStatusRequest(false);
        var expectedResponse = new UserListResponse(1, "John", "Doe", "john@example.com", "User", false, DateTime.UtcNow);

        A.CallTo(() => _userService.UpdateStatusAsync(1, request))
            .Returns(expectedResponse);

        // Act
        var result = await _controller.UpdateStatus(1, request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<UserListResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateRole_AdminUser_Returns200Ok()
    {
        // Arrange
        var request = new UpdateUserRoleRequest(1);
        var expectedResponse = new UserListResponse(1, "John", "Doe", "john@example.com", "Admin", true, DateTime.UtcNow);

        A.CallTo(() => _userService.UpdateRoleAsync(1, request))
            .Returns(expectedResponse);

        // Act
        var result = await _controller.UpdateRole(1, request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<UserListResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.RoleName.Should().Be("Admin");
    }

    [Fact]
    public async Task GetUserId_InvalidClaim_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "invalid") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        var request = new UpdateProfileRequest("John", "Doe");

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.UpdateProfile(request));
    }

    [Fact]
    public async Task UpdateStatus_NonExistingUser_ThrowsKeyNotFoundException()
    {
        // Arrange
        var request = new UpdateUserStatusRequest(false);

        A.CallTo(() => _userService.UpdateStatusAsync(999, request))
            .Throws(new KeyNotFoundException("User with ID 999 not found"));

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.UpdateStatus(999, request));
    }

    [Fact]
    public async Task UpdateRole_NonExistingRole_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new UpdateUserRoleRequest(999);

        A.CallTo(() => _userService.UpdateRoleAsync(1, request))
            .Throws(new InvalidOperationException("Role with ID 999 does not exist"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.UpdateRole(1, request));
    }

    [Fact]
    public async Task GetAll_InvalidPaginationParams_NormalizedCorrectly()
    {
        // Arrange
        var expectedResponse = new PaginatedResponse<UserListResponse>(
            new List<UserListResponse>(),
            0, 1, 10, 0
        );

        A.CallTo(() => _userService.GetAllAsync(1, 10, null))
            .Returns(expectedResponse);

        // Act - pass invalid params (0 and -1)
        var result = await _controller.GetAll(0, -1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<PaginatedResponse<UserListResponse>>>().Subject;
        response.Success.Should().BeTrue();
    }
}
