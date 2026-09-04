using CarAuction.Application.DTOs;
using CarAuction.Application.DTOs.Roles;
using CarAuction.Application.Interfaces.Services;
using CarAuction.Presentation.Controllers;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CarAuction.UnitTest.Controllers;

public class RolesPermissionsControllerTests
{
    private readonly IRolesPermissionsService _service;
    private readonly RolesPermissionsController _controller;

    public RolesPermissionsControllerTests()
    {
        _service = A.Fake<IRolesPermissionsService>();
        _controller = new RolesPermissionsController(_service);
    }

    [Fact]
    public async Task GetRoles_Returns200Ok()
    {
        // Arrange
        var expectedRoles = new List<RoleResponse>
        {
            new(1, "Admin"),
            new(2, "User")
        };

        A.CallTo(() => _service.GetAllRolesAsync())
            .Returns(expectedRoles);

        // Act
        var result = await _controller.GetRoles();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<List<RoleResponse>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(expectedRoles);
    }

    [Fact]
    public async Task CreateRole_ValidRequest_Returns201Created()
    {
        // Arrange
        var request = new CreateRoleRequest("Moderator");
        var expectedResponse = new RoleResponse(3, "Moderator");

        A.CallTo(() => _service.CreateRoleAsync(request))
            .Returns(expectedResponse);

        // Act
        var result = await _controller.CreateRole(request);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);

        var response = createdResult.Value.Should().BeOfType<ApiResponse<RoleResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task CreateRole_DuplicateName_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new CreateRoleRequest("Admin");

        A.CallTo(() => _service.CreateRoleAsync(request))
            .Throws(new InvalidOperationException("Role 'Admin' already exists"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.CreateRole(request));
    }

    [Fact]
    public async Task DeleteRole_ExistingRole_Returns200Ok()
    {
        // Arrange
        A.CallTo(() => _service.DeleteRoleAsync(1))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteRole(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Success.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteRole_NonExistingRole_ThrowsKeyNotFoundException()
    {
        // Arrange
        A.CallTo(() => _service.DeleteRoleAsync(999))
            .Throws(new KeyNotFoundException("Role with ID 999 not found"));

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.DeleteRole(999));
    }

    [Fact]
    public async Task DeleteRole_AssignedToUsers_ThrowsInvalidOperationException()
    {
        // Arrange
        A.CallTo(() => _service.DeleteRoleAsync(2))
            .Throws(new InvalidOperationException("Role 'User' is assigned to users and cannot be deleted"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.DeleteRole(2));
    }

    [Fact]
    public async Task GetPermissions_Returns200Ok()
    {
        // Arrange
        var expectedPermissions = new List<PermissionResponse>
        {
            new(1, "auction.bid", "Ability to place bids on live auctions"),
            new(2, "listing.create", "Ability to create vehicle listings"),
            new(3, "admin.manage", "Access to administrative panel and user moderation")
        };

        A.CallTo(() => _service.GetAllPermissionsAsync())
            .Returns(expectedPermissions);

        // Act
        var result = await _controller.GetPermissions();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<List<PermissionResponse>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(expectedPermissions);
    }

    [Fact]
    public async Task GetRolePermissions_ExistingRole_Returns200Ok()
    {
        // Arrange
        var expectedPermissions = new List<PermissionResponse>
        {
            new(1, "auction.bid", "Ability to place bids on live auctions")
        };

        A.CallTo(() => _service.GetRolePermissionsAsync(2))
            .Returns(expectedPermissions);

        // Act
        var result = await _controller.GetRolePermissions(2);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<List<PermissionResponse>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(expectedPermissions);
    }

    [Fact]
    public async Task GetRolePermissions_NonExistingRole_ThrowsKeyNotFoundException()
    {
        // Arrange
        A.CallTo(() => _service.GetRolePermissionsAsync(999))
            .Throws(new KeyNotFoundException("Role with ID 999 not found"));

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.GetRolePermissions(999));
    }

    [Fact]
    public async Task AssignPermission_ValidRequest_Returns200Ok()
    {
        // Arrange
        var request = new AssignPermissionRequest(1);

        A.CallTo(() => _service.AssignPermissionAsync(2, request))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.AssignPermission(2, request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Success.Should().BeTrue();
    }

    [Fact]
    public async Task AssignPermission_NonExistingRole_ThrowsKeyNotFoundException()
    {
        // Arrange
        var request = new AssignPermissionRequest(1);

        A.CallTo(() => _service.AssignPermissionAsync(999, request))
            .Throws(new KeyNotFoundException("Role with ID 999 not found"));

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.AssignPermission(999, request));
    }

    [Fact]
    public async Task AssignPermission_NonExistingPermission_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new AssignPermissionRequest(999);

        A.CallTo(() => _service.AssignPermissionAsync(2, request))
            .Throws(new InvalidOperationException("Permission with ID 999 does not exist"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.AssignPermission(2, request));
    }

    [Fact]
    public async Task RemovePermission_ValidRequest_Returns200Ok()
    {
        // Arrange
        A.CallTo(() => _service.RemovePermissionAsync(2, 1))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.RemovePermission(2, 1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Success.Should().BeTrue();
    }

    [Fact]
    public async Task RemovePermission_NonExistingRole_ThrowsKeyNotFoundException()
    {
        // Arrange
        A.CallTo(() => _service.RemovePermissionAsync(999, 1))
            .Throws(new KeyNotFoundException("Role with ID 999 not found"));

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.RemovePermission(999, 1));
    }
}
