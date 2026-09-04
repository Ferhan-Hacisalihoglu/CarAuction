using CarAuction.Application.Interfaces.Caching;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Collections.Concurrent;

namespace CarAuction.Infrastructure.Caching;

public class RedisDistributedLockService : IDistributedLockService
{
    private readonly IConnectionMultiplexer? _redis;
    private readonly ILogger<RedisDistributedLockService> _logger;
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> Semaphores = new();

    public RedisDistributedLockService(
        ILogger<RedisDistributedLockService> logger,
        IConnectionMultiplexer? redis = null)
    {
        _logger = logger;
        _redis = redis;
    }

    public async Task<IAsyncDisposable?> AcquireLockAsync(string resourceKey, TimeSpan waitTime, TimeSpan lockExpiry)
    {
        var lockKey = $"lock:{resourceKey}";

        // Attempt Redis lock if available
        if (_redis != null && _redis.IsConnected)
        {
            try
            {
                var db = _redis.GetDatabase();
                var token = Guid.NewGuid().ToString();
                var timeoutAt = DateTime.UtcNow.Add(waitTime);

                do
                {
                    var acquired = await db.LockTakeAsync(lockKey, token, lockExpiry);
                    if (acquired)
                    {
                        return new RedisLockHandle(db, lockKey, token, _logger);
                    }

                    if (DateTime.UtcNow >= timeoutAt)
                    {
                        break;
                    }

                    await Task.Delay(50);
                }
                while (DateTime.UtcNow < timeoutAt);

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis lock failed for key {Key}, falling back to in-memory semaphore", lockKey);
            }
        }

        // In-memory fallback
        var semaphore = Semaphores.GetOrAdd(resourceKey, _ => new SemaphoreSlim(1, 1));
        var entered = await semaphore.WaitAsync(waitTime);
        if (entered)
        {
            return new SemaphoreLockHandle(semaphore);
        }

        return null;
    }

    private sealed class RedisLockHandle : IAsyncDisposable
    {
        private readonly IDatabase _db;
        private readonly string _key;
        private readonly string _token;
        private readonly ILogger _logger;
        private int _disposed;

        public RedisLockHandle(IDatabase db, string key, string token, ILogger logger)
        {
            _db = db;
            _key = key;
            _token = token;
            _logger = logger;
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                try
                {
                    await _db.LockReleaseAsync(_key, _token);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to release Redis lock {Key}", _key);
                }
            }
        }
    }

    private sealed class SemaphoreLockHandle : IAsyncDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        private int _disposed;

        public SemaphoreLockHandle(SemaphoreSlim semaphore)
        {
            _semaphore = semaphore;
        }

        public ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                _semaphore.Release();
            }
            return ValueTask.CompletedTask;
        }
    }
}
