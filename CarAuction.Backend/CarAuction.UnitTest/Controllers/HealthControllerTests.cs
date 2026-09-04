using CarAuction.Infrastructure.Data.Connection;
using CarAuction.Presentation.Controllers;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using StackExchange.Redis;
using Xunit;

namespace CarAuction.UnitTest.Controllers;

public class HealthControllerTests
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly IConnectionMultiplexer _redis;
    private readonly HealthController _controller;

    public HealthControllerTests()
    {
        _connectionFactory = A.Fake<IConnectionFactory>();
        _redis = A.Fake<IConnectionMultiplexer>();
        _controller = new HealthController(
            _connectionFactory,
            NullLogger<HealthController>.Instance,
            _redis
        );
    }

    [Fact]
    public void Live_Returns200Ok_WithHealthyStatus()
    {
        // Act
        var result = _controller.Live();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task Ready_WhenPostgresAndRedisHealthy_Returns200Ok()
    {
        // Arrange
        A.CallTo(() => _connectionFactory.CanConnectAsync()).Returns(true);
        A.CallTo(() => _redis.IsConnected).Returns(true);

        // Act
        var result = await _controller.Ready();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task Ready_WhenPostgresUnhealthy_Returns503ServiceUnavailable()
    {
        // Arrange
        A.CallTo(() => _connectionFactory.CanConnectAsync()).Returns(false);
        A.CallTo(() => _redis.IsConnected).Returns(true);

        // Act
        var result = await _controller.Ready();

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
    }

    [Fact]
    public async Task Ready_WhenRedisUnhealthy_Returns503ServiceUnavailable()
    {
        // Arrange
        A.CallTo(() => _connectionFactory.CanConnectAsync()).Returns(true);
        A.CallTo(() => _redis.IsConnected).Returns(false);

        // Act
        var result = await _controller.Ready();

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
    }
}
