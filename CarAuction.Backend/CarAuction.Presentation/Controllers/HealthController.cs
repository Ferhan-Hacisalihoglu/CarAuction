using CarAuction.Infrastructure.Data.Connection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace CarAuction.Presentation.Controllers;

[ApiController]
[Route("health")]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly IConnectionMultiplexer? _redis;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        IConnectionFactory connectionFactory,
        ILogger<HealthController> logger,
        IConnectionMultiplexer? redis = null)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
        _redis = redis;
    }

    [HttpGet("live")]
    [AllowAnonymous]
    public IActionResult Live()
    {
        return Ok(new
        {
            status = "Healthy",
            timestamp = DateTime.UtcNow
        });
    }

    [HttpGet("ready")]
    [AllowAnonymous]
    public async Task<IActionResult> Ready()
    {
        var isDbHealthy = false;
        var isRedisHealthy = false;

        try
        {
            isDbHealthy = await _connectionFactory.CanConnectAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Readiness probe: Database connection error");
        }

        try
        {
            isRedisHealthy = _redis != null && _redis.IsConnected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Readiness probe: Redis connection error");
        }

        var result = new
        {
            status = isDbHealthy && isRedisHealthy ? "Healthy" : "Unhealthy",
            database = isDbHealthy ? "Connected" : "Disconnected",
            redis = isRedisHealthy ? "Connected" : "Disconnected",
            timestamp = DateTime.UtcNow
        };

        if (isDbHealthy && isRedisHealthy)
        {
            return Ok(result);
        }

        return StatusCode(StatusCodes.Status503ServiceUnavailable, result);
    }
}
