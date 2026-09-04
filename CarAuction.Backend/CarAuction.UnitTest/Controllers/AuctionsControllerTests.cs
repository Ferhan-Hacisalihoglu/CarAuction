using CarAuction.Application.DTOs;
using CarAuction.Application.DTOs.Auctions;
using CarAuction.Application.Interfaces.Services;
using CarAuction.Presentation.Controllers;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CarAuction.UnitTest.Controllers;

public class AuctionsControllerTests
{
    private readonly IAuctionService _auctionService;
    private readonly AuctionsController _controller;

    public AuctionsControllerTests()
    {
        _auctionService = A.Fake<IAuctionService>();
        _controller = new AuctionsController(_auctionService);
    }

    [Fact]
    public async Task GetActive_Returns200Ok()
    {
        // Arrange
        var expectedAuctions = new List<AuctionListItemResponse>
        {
            new(1, 1, "Tesla Model S", "Electric car", 1, 50000, 50000, DateTime.UtcNow, DateTime.UtcNow.AddDays(7), 1000, "active"),
            new(2, 2, "BMW M3", "Sport car", 2, 60000, 60000, DateTime.UtcNow, DateTime.UtcNow.AddDays(5), 1000, "active")
        };

        A.CallTo(() => _auctionService.GetActiveAuctionsAsync())
            .Returns(expectedAuctions);

        // Act
        var result = await _controller.GetActive();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<List<AuctionListItemResponse>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(expectedAuctions);
    }

    [Fact]
    public async Task GetById_Existing_Returns200Ok()
    {
        // Arrange
        var expectedAuction = new AuctionDetailResponse(
            1, 1, "Tesla Model S", "Electric car", 1, 50000, 50000,
            DateTime.UtcNow, DateTime.UtcNow.AddDays(7), 1000, null, "active");

        A.CallTo(() => _auctionService.GetByIdAsync(1))
            .Returns(expectedAuction);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<AuctionDetailResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(expectedAuction);
    }

    [Fact]
    public async Task GetById_NonExisting_Returns404NotFound()
    {
        // Arrange
        A.CallTo(() => _auctionService.GetByIdAsync(999))
            .Returns((AuctionDetailResponse?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);

        var response = notFoundResult.Value.Should().BeOfType<ApiResponse<AuctionDetailResponse>>().Subject;
        response.Success.Should().BeFalse();
    }

    [Fact]
    public async Task GetByListingId_Existing_Returns200Ok()
    {
        // Arrange
        var expectedAuction = new AuctionDetailResponse(
            1, 1, "Tesla Model S", "Electric car", 1, 50000, 50000,
            DateTime.UtcNow, DateTime.UtcNow.AddDays(7), 1000, null, "active");

        A.CallTo(() => _auctionService.GetByListingIdAsync(1))
            .Returns(expectedAuction);

        // Act
        var result = await _controller.GetByListingId(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<AuctionDetailResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(expectedAuction);
    }

    [Fact]
    public async Task GetByListingId_NonExisting_Returns404NotFound()
    {
        // Arrange
        A.CallTo(() => _auctionService.GetByListingIdAsync(999))
            .Returns((AuctionDetailResponse?)null);

        // Act
        var result = await _controller.GetByListingId(999);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);

        var response = notFoundResult.Value.Should().BeOfType<ApiResponse<AuctionDetailResponse>>().Subject;
        response.Success.Should().BeFalse();
    }

    [Fact]
    public async Task PlaceBid_Authenticated_Returns200Ok()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        var request = new PlaceBidRequest(51000);
        var expectedResponse = new AuctionDetailResponse(
            1, 1, "Tesla Model S", "Electric car", 1, 50000, 51000,
            DateTime.UtcNow, DateTime.UtcNow.AddDays(7), 1000, 1, "active");

        A.CallTo(() => _auctionService.PlaceBidAsync(1, 1, request, A<string>._))
            .Returns(expectedResponse);

        // Act
        var result = await _controller.PlaceBid(1, request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<AuctionDetailResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.CurrentPrice.Should().Be(51000);
    }

    [Fact]
    public async Task PlaceBid_NonExistingAuction_ThrowsKeyNotFoundException()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        var request = new PlaceBidRequest(51000);

        A.CallTo(() => _auctionService.PlaceBidAsync(999, 1, request, A<string>._))
            .Throws(new KeyNotFoundException("Auction with ID 999 not found"));

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.PlaceBid(999, request));
    }

    [Fact]
    public async Task PlaceBid_AuctionNotActive_ThrowsInvalidOperationException()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        var request = new PlaceBidRequest(51000);

        A.CallTo(() => _auctionService.PlaceBidAsync(1, 1, request, A<string>._))
            .Throws(new InvalidOperationException("Auction is not active"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.PlaceBid(1, request));
    }

    [Fact]
    public async Task PlaceBid_MinimumBidNotMet_ThrowsInvalidOperationException()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        var request = new PlaceBidRequest(50000);

        A.CallTo(() => _auctionService.PlaceBidAsync(1, 1, request, A<string>._))
            .Throws(new InvalidOperationException("Minimum bid is 51000"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.PlaceBid(1, request));
    }

    [Fact]
    public async Task GetBidHistory_ExistingAuction_Returns200Ok()
    {
        // Arrange
        var expectedBids = new List<BidHistoryResponse>
        {
            new(1, 1, 1, "John Doe", 51000, DateTime.UtcNow),
            new(2, 1, 2, "Jane Smith", 52000, DateTime.UtcNow.AddMinutes(-5))
        };

        A.CallTo(() => _auctionService.GetBidHistoryAsync(1))
            .Returns(expectedBids);

        // Act
        var result = await _controller.GetBidHistory(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<List<BidHistoryResponse>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(expectedBids);
    }

    [Fact]
    public async Task GetBidHistory_NonExistingAuction_ThrowsKeyNotFoundException()
    {
        // Arrange
        A.CallTo(() => _auctionService.GetBidHistoryAsync(999))
            .Throws(new KeyNotFoundException("Auction with ID 999 not found"));

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.GetBidHistory(999));
    }

    [Fact]
    public async Task GetUserId_InvalidClaim_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "invalid") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        var request = new PlaceBidRequest(51000);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.PlaceBid(1, request));
    }
}
