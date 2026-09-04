using CarAuction.Application.DTOs;
using CarAuction.Application.DTOs.Listings;
using CarAuction.Application.DTOs.Users;
using CarAuction.Application.Interfaces.Services;
using CarAuction.Presentation.Controllers;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CarAuction.UnitTest.Controllers;

public class ListingsControllerTests
{
    private readonly IListingService _listingService;
    private readonly ListingsController _controller;

    public ListingsControllerTests()
    {
        _listingService = A.Fake<IListingService>();
        _controller = new ListingsController(_listingService);
    }

    [Fact]
    public async Task GetAll_Public_Returns200Ok()
    {
        // Arrange
        var expectedResponse = new PaginatedResponse<ListingListItemResponse>(
            new List<ListingListItemResponse>
            {
                new(1, 1, "Tesla Model S", "Electric car", 50000, false, "active", DateTime.UtcNow),
                new(2, 2, "BMW M3", "Sport car", 60000, true, "active", DateTime.UtcNow)
            },
            2, 1, 10, 1
        );

        A.CallTo(() => _listingService.GetAllAsync(A<ListingFilters>._))
            .Returns(expectedResponse);

        // Act
        var result = await _controller.GetAll(null, null, null, null, null, 1, 10);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<PaginatedResponse<ListingListItemResponse>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetById_Existing_Returns200Ok()
    {
        // Arrange
        var expectedListing = new ListingDetailResponse(
            1, 1, "Tesla Model S", "Electric car", 50000, false, "active",
            DateTime.UtcNow, null, new List<ImageResponse>());

        A.CallTo(() => _listingService.GetByIdAsync(1))
            .Returns(expectedListing);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<ListingDetailResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(expectedListing);
    }

    [Fact]
    public async Task GetById_NonExisting_Returns404NotFound()
    {
        // Arrange
        A.CallTo(() => _listingService.GetByIdAsync(999))
            .Returns((ListingDetailResponse?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);

        var response = notFoundResult.Value.Should().BeOfType<ApiResponse<ListingDetailResponse>>().Subject;
        response.Success.Should().BeFalse();
    }

    [Fact]
    public async Task Create_Authenticated_Returns201Created()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        var request = new CreateListingRequest("Tesla Model S", "Electric car", 50000, false, null, null, null, null);
        var expectedResponse = new ListingDetailResponse(
            1, 1, "Tesla Model S", "Electric car", 50000, false, "active",
            DateTime.UtcNow, null, new List<ImageResponse>());

        A.CallTo(() => _listingService.CreateAsync(1, request))
            .Returns(expectedResponse);

        // Act
        var result = await _controller.Create(request);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);

        var response = createdResult.Value.Should().BeOfType<ApiResponse<ListingDetailResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.Title.Should().Be("Tesla Model S");
    }

    [Fact]
    public async Task Update_Authenticated_Returns200Ok()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        var request = new UpdateListingRequest("Updated Title", "Updated desc", 55000);
        var expectedResponse = new ListingDetailResponse(
            1, 1, "Updated Title", "Updated desc", 55000, false, "active",
            DateTime.UtcNow, DateTime.UtcNow, new List<ImageResponse>());

        A.CallTo(() => _listingService.UpdateAsync(1, 1, request))
            .Returns(expectedResponse);

        // Act
        var result = await _controller.Update(1, request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<ListingDetailResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.Title.Should().Be("Updated Title");
    }

    [Fact]
    public async Task Update_NotOwner_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "2") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        var request = new UpdateListingRequest("Updated Title", "Updated desc", 55000);

        A.CallTo(() => _listingService.UpdateAsync(1, 2, request))
            .Throws(new UnauthorizedAccessException("You are not the owner of this listing"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.Update(1, request));
    }

    [Fact]
    public async Task Delete_Authenticated_Returns200Ok()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        A.CallTo(() => _listingService.DeleteAsync(1, 1))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_NotOwner_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "2") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        A.CallTo(() => _listingService.DeleteAsync(1, 2))
            .Throws(new UnauthorizedAccessException("You are not the owner of this listing"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.Delete(1));
    }

    [Fact]
    public async Task GetMyListings_Authenticated_Returns200Ok()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        var expectedListings = new List<ListingListItemResponse>
        {
            new(1, 1, "Tesla Model S", "Electric car", 50000, false, "active", DateTime.UtcNow),
            new(2, 1, "BMW M3", "Sport car", 60000, true, "active", DateTime.UtcNow)
        };

        A.CallTo(() => _listingService.GetMyListingsAsync(1))
            .Returns(expectedListings);

        // Act
        var result = await _controller.GetMyListings();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<List<ListingListItemResponse>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(expectedListings);
    }

    [Fact]
    public async Task GetUserId_InvalidClaim_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "invalid") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        var request = new CreateListingRequest("Test", "Desc", 1000, false, null, null, null, null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.Create(request));
    }
}
