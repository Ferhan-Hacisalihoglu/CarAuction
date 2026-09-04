using CarAuction.Infrastructure.Caching;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CarAuction.UnitTest.Infrastructure;

public class RedisServicesTests
{
    [Fact]
    public async Task RedisCacheService_GetOrSetAsync_WorksWithFallback()
    {
        // Arrange
        var cacheService = new RedisCacheService(NullLogger<RedisCacheService>.Instance, null);
        var factoryCallCount = 0;

        // Act
        var firstResult = await cacheService.GetOrSetAsync("test:key", () =>
        {
            factoryCallCount++;
            return Task.FromResult("calculated_value");
        }, TimeSpan.FromMinutes(1));

        var secondResult = await cacheService.GetOrSetAsync("test:key", () =>
        {
            factoryCallCount++;
            return Task.FromResult("recalculated_value");
        }, TimeSpan.FromMinutes(1));

        // Assert
        firstResult.Should().Be("calculated_value");
        secondResult.Should().Be("calculated_value");
        factoryCallCount.Should().Be(1);
    }

    [Fact]
    public async Task RedisCacheService_RemoveByPrefixAsync_ClearsMatchingKeys()
    {
        // Arrange
        var cacheService = new RedisCacheService(NullLogger<RedisCacheService>.Instance, null);
        await cacheService.SetAsync("listings:item:1", "data1");
        await cacheService.SetAsync("listings:item:2", "data2");
        await cacheService.SetAsync("users:1", "user1");

        // Act
        await cacheService.RemoveByPrefixAsync("listings:");

        // Assert
        var item1 = await cacheService.GetAsync<string>("listings:item:1");
        var item2 = await cacheService.GetAsync<string>("listings:item:2");
        var user = await cacheService.GetAsync<string>("users:1");

        item1.Should().BeNull();
        item2.Should().BeNull();
        user.Should().Be("user1");
    }

    [Fact]
    public async Task RedisDistributedLockService_AcquireAndDisposeLock_AllowsSubsequentLock()
    {
        // Arrange
        var lockService = new RedisDistributedLockService(NullLogger<RedisDistributedLockService>.Instance, null);

        // Act - Acquire first lock
        var handle1 = await lockService.AcquireLockAsync("auction:100", TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(5));
        handle1.Should().NotBeNull();

        // Release first lock
        await handle1!.DisposeAsync();

        // Acquire second lock on same resource
        var handle2 = await lockService.AcquireLockAsync("auction:100", TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(5));

        // Assert
        handle2.Should().NotBeNull();
        await handle2!.DisposeAsync();
    }
}
