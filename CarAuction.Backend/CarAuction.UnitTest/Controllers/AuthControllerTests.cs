using CarAuction.Application.DTOs;
using CarAuction.Application.DTOs.Auth;
using CarAuction.Application.Interfaces.Services;
using CarAuction.Presentation.Controllers;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CarAuction.UnitTest.Controllers;

public class AuthControllerTests
{
    private readonly IAuthService _authService;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authService = A.Fake<IAuthService>();
        _controller = new AuthController(_authService);
    }

    [Fact]
    public async Task Register_ValidRequest_Returns201Created()
    {
        // Arrange
        var request = new RegisterRequest("John", "Doe", "john@example.com", "password123");
        var expectedResponse = new AuthResponse(
            "access-token",
            "refresh-token",
            new UserDto(1, "John", "Doe", "john@example.com", "User")
        );

        A.CallTo(() => _authService.RegisterAsync(request))
            .Returns(expectedResponse);

        // Act
        var result = await _controller.Register(request);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);

        var response = createdResult.Value.Should().BeOfType<ApiResponse<AuthResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new RegisterRequest("John", "Doe", "john@example.com", "password123");

        A.CallTo(() => _authService.RegisterAsync(request))
            .Throws(new InvalidOperationException("Email already registered"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Register(request));
    }

    [Fact]
    public async Task Login_ValidCredentials_Returns200Ok()
    {
        // Arrange
        var request = new LoginRequest("john@example.com", "password123");
        var expectedResponse = new AuthResponse(
            "access-token",
            "refresh-token",
            new UserDto(1, "John", "Doe", "john@example.com", "User")
        );

        A.CallTo(() => _authService.LoginAsync(request))
            .Returns(expectedResponse);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<AuthResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var request = new LoginRequest("john@example.com", "wrongpassword");

        A.CallTo(() => _authService.LoginAsync(request))
            .Throws(new UnauthorizedAccessException("Invalid email or password"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.Login(request));
    }

    [Fact]
    public async Task RefreshToken_ValidToken_Returns200Ok()
    {
        // Arrange
        var request = new RefreshTokenRequest("valid-refresh-token");
        var expectedResponse = new AuthResponse(
            "new-access-token",
            "new-refresh-token",
            new UserDto(1, "John", "Doe", "john@example.com", "User")
        );

        A.CallTo(() => _authService.RefreshTokenAsync(request.Token))
            .Returns(expectedResponse);

        // Act
        var result = await _controller.RefreshToken(request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<AuthResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.AccessToken.Should().Be("new-access-token");
        response.Data.RefreshToken.Should().Be("new-refresh-token");
    }

    [Fact]
    public async Task RefreshToken_ExpiredToken_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var request = new RefreshTokenRequest("expired-refresh-token");

        A.CallTo(() => _authService.RefreshTokenAsync(request.Token))
            .Throws(new UnauthorizedAccessException("Refresh token expired"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.RefreshToken(request));
    }

    [Fact]
    public async Task Logout_AuthenticatedUser_Returns200Ok()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        A.CallTo(() => _authService.LogoutAsync(1))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Logout();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Me_AuthenticatedUser_Returns200Ok()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        var expectedUser = new UserDto(1, "John", "Doe", "john@example.com", "User");

        A.CallTo(() => _authService.GetCurrentUserAsync(1))
            .Returns(expectedUser);

        // Act
        var result = await _controller.Me();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<UserDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(expectedUser);
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
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.Logout());
    }
}
