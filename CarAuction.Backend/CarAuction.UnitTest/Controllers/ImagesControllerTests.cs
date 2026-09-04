using CarAuction.Application.DTOs;
using CarAuction.Application.DTOs.Images;
using CarAuction.Application.Interfaces.Services;
using CarAuction.Presentation.Controllers;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CarAuction.UnitTest.Controllers;

public class ImagesControllerTests
{
    private readonly IImageService _imageService;
    private readonly ImagesController _controller;

    public ImagesControllerTests()
    {
        _imageService = A.Fake<IImageService>();
        _controller = new ImagesController(_imageService);
    }

    [Fact]
    public async Task Upload_ValidFile_Returns201Created()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        var file = new FormFile(
            new MemoryStream(new byte[] { 1, 2, 3, 4 }),
            0, 4, "file", "test.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };

        var expectedResponse = new UploadImageResponse(1, "test.jpg", "image/jpeg", 4, DateTime.UtcNow);

        A.CallTo(() => _imageService.UploadAsync(1, 1, A<Stream>._, "test.jpg", "image/jpeg"))
            .Returns(expectedResponse);

        // Act
        var result = await _controller.Upload(1, file);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);

        var response = createdResult.Value.Should().BeOfType<ApiResponse<UploadImageResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.FileName.Should().Be("test.jpg");
    }

    [Fact]
    public async Task Upload_NoFile_Returns400BadRequest()
    {
        // Act
        var result = await _controller.Upload(1, null);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);

        var response = badRequestResult.Value.Should().BeOfType<ApiResponse<UploadImageResponse>>().Subject;
        response.Success.Should().BeFalse();
    }

    [Fact]
    public async Task Upload_EmptyFile_Returns400BadRequest()
    {
        // Arrange
        var file = new FormFile(new MemoryStream(), 0, 0, "file", "test.jpg")
        {
            Headers = new HeaderDictionary()
        };

        // Act
        var result = await _controller.Upload(1, file);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);

        var response = badRequestResult.Value.Should().BeOfType<ApiResponse<UploadImageResponse>>().Subject;
        response.Success.Should().BeFalse();
    }

    [Fact]
    public async Task Upload_NonExistingListing_ThrowsKeyNotFoundException()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        var file = new FormFile(
            new MemoryStream(new byte[] { 1, 2, 3 }),
            0, 3, "file", "test.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };

        A.CallTo(() => _imageService.UploadAsync(999, 1, A<Stream>._, "test.jpg", "image/jpeg"))
            .Throws(new KeyNotFoundException("Listing with ID 999 not found"));

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.Upload(999, file));
    }

    [Fact]
    public async Task Upload_NotOwner_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "2") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        var file = new FormFile(
            new MemoryStream(new byte[] { 1, 2, 3 }),
            0, 3, "file", "test.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };

        A.CallTo(() => _imageService.UploadAsync(1, 2, A<Stream>._, "test.jpg", "image/jpeg"))
            .Throws(new UnauthorizedAccessException("You are not the owner of this listing"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.Upload(1, file));
    }

    [Fact]
    public async Task Upload_InvalidFormat_ThrowsInvalidOperationException()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        var file = new FormFile(
            new MemoryStream(new byte[] { 1, 2, 3 }),
            0, 3, "file", "test.pdf")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/pdf"
        };

        A.CallTo(() => _imageService.UploadAsync(1, 1, A<Stream>._, "test.pdf", "application/pdf"))
            .Throws(new InvalidOperationException("Invalid image format"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Upload(1, file));
    }

    [Fact]
    public async Task GetById_Existing_ReturnsFile()
    {
        // Arrange
        var imageData = new byte[] { 1, 2, 3, 4, 5 };
        A.CallTo(() => _imageService.GetByIdAsync(1))
            .Returns((imageData, "image/jpeg", "test.jpg"));

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
        fileResult.FileContents.Should().BeEquivalentTo(imageData);
        fileResult.ContentType.Should().Be("image/jpeg");
        fileResult.FileDownloadName.Should().Be("test.jpg");
    }

    [Fact]
    public async Task GetById_NonExisting_ThrowsKeyNotFoundException()
    {
        // Arrange
        A.CallTo(() => _imageService.GetByIdAsync(999))
            .Throws(new KeyNotFoundException("Image with ID 999 not found"));

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.GetById(999));
    }

    [Fact]
    public async Task Delete_Authenticated_Returns200Ok()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        A.CallTo(() => _imageService.DeleteAsync(1, 1, false))
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

        A.CallTo(() => _imageService.DeleteAsync(1, 2, false))
            .Throws(new UnauthorizedAccessException("You are not the owner of this listing"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.Delete(1));
    }

    [Fact]
    public async Task Delete_NonExisting_ThrowsKeyNotFoundException()
    {
        // Arrange
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };

        A.CallTo(() => _imageService.DeleteAsync(999, 1, false))
            .Throws(new KeyNotFoundException("Image with ID 999 not found"));

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.Delete(999));
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
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.Delete(1));
    }
}
