using CarAuction.Application.DTOs;
using CarAuction.Application.DTOs.Bids;
using CarAuction.Application.Interfaces.Services;
using CarAuction.Presentation.Controllers;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CarAuction.UnitTest.Controllers;

public class BidsControllerTests
{
    private readonly IBidsService _bidsService;
    private readonly BidsController _controller;

    public BidsControllerTests()
    {
        _bidsService = A.Fake<IBidsService>();
        _controller = new BidsController(_bidsService);
    }

    [Fact]
    public async Task MakeOffer_Authenticated_Returns201Created()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "2") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        var request = new MakeOfferRequest(45000);
        var expectedResponse = new OfferResponse(
            1, 1, "Tesla Model S", 2, "Jane Smith", 45000, DateTime.UtcNow);

        A.CallTo(() => _bidsService.MakeOfferAsync(1, 2, request))
            .Returns(expectedResponse);

        // Act
        var result = await _controller.MakeOffer(1, request);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);

        var response = createdResult.Value.Should().BeOfType<ApiResponse<OfferResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.Amount.Should().Be(45000);
    }

    [Fact]
    public async Task MakeOffer_OwnListing_ThrowsInvalidOperationException()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        var request = new MakeOfferRequest(45000);

        A.CallTo(() => _bidsService.MakeOfferAsync(1, 1, request))
            .Throws(new InvalidOperationException("You cannot make an offer on your own listing"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.MakeOffer(1, request));
    }

    [Fact]
    public async Task GetOffers_Owner_Returns200Ok()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        var expectedOffers = new List<OfferResponse>
        {
            new(1, 1, "Tesla Model S", 2, "Jane Smith", 45000, DateTime.UtcNow),
            new(2, 1, "Tesla Model S", 3, "Bob Johnson", 48000, DateTime.UtcNow.AddMinutes(-10))
        };

        A.CallTo(() => _bidsService.GetOffersAsync(1, 1))
            .Returns(expectedOffers);

        // Act
        var result = await _controller.GetOffers(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<List<OfferResponse>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(expectedOffers);
    }

    [Fact]
    public async Task GetOffers_NotOwner_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "2") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        A.CallTo(() => _bidsService.GetOffersAsync(1, 2))
            .Throws(new UnauthorizedAccessException("You are not the owner of this listing"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.GetOffers(1));
    }

    [Fact]
    public async Task GetMyBids_Authenticated_Returns200Ok()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "2") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        var expectedBids = new List<MyBidResponse>
        {
            new(1, 1, "Tesla Model S", 50000, "active", 45000, DateTime.UtcNow),
            new(2, 3, "Ford Mustang", 40000, "active", 38000, DateTime.UtcNow.AddHours(-2))
        };

        A.CallTo(() => _bidsService.GetMyBidsAsync(2))
            .Returns(expectedBids);

        // Act
        var result = await _controller.GetMyBids();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<List<MyBidResponse>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(expectedBids);
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
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.GetMyBids());
    }
}
