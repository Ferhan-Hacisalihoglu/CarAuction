using CarAuction.Application.Interfaces.Caching;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Text.Json;

namespace CarAuction.Infrastructure.Caching;

public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer? _redis;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly ConcurrentDictionary<string, (string Value, DateTime? Expiry)> _memoryFallback = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public RedisCacheService(ILogger<RedisCacheService> logger, IConnectionMultiplexer? redis = null)
    {
        _logger = logger;
        _redis = redis;
    }

    private IDatabase? GetDatabase()
    {
        if (_redis != null && _redis.IsConnected)
        {
            return _redis.GetDatabase();
        }
        return null;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var stringVal = await GetStringAsync(key);
        if (string.IsNullOrEmpty(stringVal))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(stringVal, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize cached value for key {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        if (value == null)
        {
            await RemoveAsync(key);
            return;
        }

        var stringVal = JsonSerializer.Serialize(value, JsonOptions);
        await SetStringAsync(key, stringVal, expiry);
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            var db = GetDatabase();
            if (db != null)
            {
                await db.KeyDeleteAsync(key);
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis error deleting key {Key}, falling back to memory", key);
        }

        _memoryFallback.TryRemove(key, out _);
    }

    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            var db = GetDatabase();
            if (db != null)
            {
                return await db.KeyExistsAsync(key);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis error checking existence of key {Key}, falling back to memory", key);
        }

        if (_memoryFallback.TryGetValue(key, out var entry))
        {
            if (entry.Expiry == null || entry.Expiry > DateTime.UtcNow)
            {
                return true;
            }
            _memoryFallback.TryRemove(key, out _);
        }

        return false;
    }

    public async Task<string?> GetStringAsync(string key)
    {
        try
        {
            var db = GetDatabase();
            if (db != null)
            {
                RedisValue val = await db.StringGetAsync(key);
                if (val.HasValue)
                {
                    return val.ToString();
                }
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis error reading key {Key}, falling back to memory", key);
        }

        if (_memoryFallback.TryGetValue(key, out var entry))
        {
            if (entry.Expiry == null || entry.Expiry > DateTime.UtcNow)
            {
                return entry.Value;
            }
            _memoryFallback.TryRemove(key, out _);
        }

        return null;
    }

    public async Task SetStringAsync(string key, string value, TimeSpan? expiry = null)
    {
        try
        {
            var db = GetDatabase();
            if (db != null)
            {
                await db.StringSetAsync(key, value, expiry);
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis error writing key {Key}, falling back to memory", key);
        }

        DateTime? expireAt = expiry.HasValue ? DateTime.UtcNow.Add(expiry.Value) : null;
        _memoryFallback[key] = (value, expireAt);
    }
}
