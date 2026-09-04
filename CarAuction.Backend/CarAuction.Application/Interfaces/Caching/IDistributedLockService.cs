namespace CarAuction.Application.Interfaces.Caching;

public interface IDistributedLockService
{
    Task<IAsyncDisposable?> AcquireLockAsync(string resourceKey, TimeSpan waitTime, TimeSpan lockExpiry);
}
