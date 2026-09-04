using CarAuction.Application.DTOs.Auctions;
using CarAuction.Application.Interfaces.Caching;
using CarAuction.Application.Interfaces.Repositories;
using CarAuction.Application.Interfaces.Services;
using CarAuction.Infrastructure.Data.Connection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace CarAuction.Infrastructure.BackgroundServices;

public class AuctionExpiryWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuctionExpiryWorker> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(5);

    public AuctionExpiryWorker(IServiceProvider serviceProvider, ILogger<AuctionExpiryWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Auction Expiry Worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndExpireAuctionsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while checking expired auctions");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task CheckAndExpireAuctionsAsync(CancellationToken stoppingToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var lockService = scope.ServiceProvider.GetService<IDistributedLockService>();

        IAsyncDisposable? workerLock = null;
        if (lockService != null)
        {
            workerLock = await lockService.AcquireLockAsync("auction:expiry-worker", TimeSpan.Zero, TimeSpan.FromSeconds(10));
            if (workerLock == null)
            {
                // Another node is actively running the expiry checks
                return;
            }
        }

        try
        {
            var connectionFactory = scope.ServiceProvider.GetRequiredService<IConnectionFactory>();
            var cacheService = scope.ServiceProvider.GetService<ICacheService>();
            var notificationService = scope.ServiceProvider.GetService<INotificationService>();
            var userRepository = scope.ServiceProvider.GetService<IUserRepository>();

            await using var connection = await connectionFactory.CreateConnectionAsync();

            // Find expired auctions
            await using var selectCommand = new NpgsqlCommand(
                @"SELECT a.id, a.listing_id, a.current_price, a.winner_user_id
                  FROM auctions a
                  INNER JOIN listings l ON a.listing_id = l.id
                  WHERE a.end_time <= NOW() AND a.status = 'active'", connection);

        await using var reader = await selectCommand.ExecuteReaderAsync();
        var expiredAuctions = new List<(int Id, int ListingId, decimal CurrentPrice, int? WinnerUserId)>();

        while (await reader.ReadAsync())
        {
            expiredAuctions.Add((
                reader.GetFieldValue<int>(reader.GetOrdinal("id")),
                reader.GetFieldValue<int>(reader.GetOrdinal("listing_id")),
                reader.GetFieldValue<decimal>(reader.GetOrdinal("current_price")),
                reader.IsDBNull(reader.GetOrdinal("winner_user_id")) ? null : reader.GetFieldValue<int?>(reader.GetOrdinal("winner_user_id"))
            ));
        }
        await reader.CloseAsync();

        foreach (var auction in expiredAuctions)
        {
            await using var transaction = await connection.BeginTransactionAsync(stoppingToken);

            try
            {
                var resultStatus = "expired";
                if (auction.WinnerUserId.HasValue)
                {
                    resultStatus = "completed";
                    // Complete auction
                    await using var updateAuctionCommand = new NpgsqlCommand(
                        "UPDATE auctions SET status = 'completed' WHERE id = @id", connection, transaction);
                    updateAuctionCommand.Parameters.Add(new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = auction.Id });
                    await updateAuctionCommand.ExecuteNonQueryAsync(stoppingToken);

                    // Mark listing as sold
                    await using var updateListingCommand = new NpgsqlCommand(
                        "UPDATE listings SET status = 'sold', updated_at = NOW() WHERE id = @id", connection, transaction);
                    updateListingCommand.Parameters.Add(new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = auction.ListingId });
                    await updateListingCommand.ExecuteNonQueryAsync(stoppingToken);

                    _logger.LogInformation("Auction {AuctionId} completed with winner {WinnerUserId}", auction.Id, auction.WinnerUserId);
                }
                else
                {
                    // Expire auction
                    await using var updateAuctionCommand = new NpgsqlCommand(
                        "UPDATE auctions SET status = 'expired' WHERE id = @id", connection, transaction);
                    updateAuctionCommand.Parameters.Add(new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = auction.Id });
                    await updateAuctionCommand.ExecuteNonQueryAsync(stoppingToken);

                    // Mark listing as expired
                    await using var updateListingCommand = new NpgsqlCommand(
                        "UPDATE listings SET status = 'expired', updated_at = NOW() WHERE id = @id", connection, transaction);
                    updateListingCommand.Parameters.Add(new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = auction.ListingId });
                    await updateListingCommand.ExecuteNonQueryAsync(stoppingToken);

                    _logger.LogInformation("Auction {AuctionId} expired with no bids", auction.Id);
                }

                await transaction.CommitAsync(stoppingToken);

                // Invalidate / update Redis cache
                if (cacheService != null)
                {
                    await cacheService.RemoveAsync($"auction:{auction.Id}:state");
                    await cacheService.RemoveAsync("auctions:active");
                    await cacheService.RemoveByPrefixAsync("listings:catalog");
                    await cacheService.RemoveAsync($"listing:{auction.ListingId}");
                }

                // Broadcast AuctionEnded event via SignalR Hub
                if (notificationService != null)
                {
                    string? winnerName = null;
                    if (auction.WinnerUserId.HasValue && userRepository != null)
                    {
                        var winner = await userRepository.GetByIdAsync(auction.WinnerUserId.Value);
                        winnerName = winner != null ? $"{winner.FirstName} {winner.LastName}" : $"User #{auction.WinnerUserId.Value}";
                    }

                    await notificationService.NotifyAuctionEndedAsync(auction.Id, new AuctionResultDto(
                        auction.Id,
                        auction.WinnerUserId,
                        winnerName,
                        auction.CurrentPrice,
                        resultStatus
                    ));
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(stoppingToken);
                _logger.LogError(ex, "Error processing expired auction {AuctionId}", auction.Id);
            }
        }
        }
        finally
        {
            if (workerLock != null)
            {
                await workerLock.DisposeAsync();
            }
        }
    }
}
