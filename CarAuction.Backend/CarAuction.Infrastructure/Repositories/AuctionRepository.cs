using CarAuction.Application.DTOs.Auctions;
using CarAuction.Application.Interfaces.Repositories;
using CarAuction.Infrastructure.Data.Connection;
using Npgsql;
using NpgsqlTypes;

namespace CarAuction.Infrastructure.Repositories;

public class AuctionRepository : IAuctionRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public AuctionRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<AuctionListItemResponse>> GetActiveAuctionsAsync()
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            @"SELECT a.id, a.listing_id, l.title, l.description, l.user_id as seller_id,
                     a.starting_price, a.current_price, a.start_time, a.end_time,
                     a.min_bid_increment, a.status
              FROM auctions a
              INNER JOIN listings l ON a.listing_id = l.id
              WHERE a.end_time > NOW() AND a.status = 'active'
              ORDER BY a.end_time ASC", connection);

        var auctions = new List<AuctionListItemResponse>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            auctions.Add(MapAuctionListItem(reader));
        }

        return auctions;
    }

    public async Task<AuctionDetailResponse?> GetByIdAsync(int id)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            @"SELECT a.id, a.listing_id, l.title, l.description, l.user_id as seller_id,
                     a.starting_price, a.current_price, a.start_time, a.end_time,
                     a.min_bid_increment, a.winner_user_id, a.status
              FROM auctions a
              INNER JOIN listings l ON a.listing_id = l.id
              WHERE a.id = @id", connection);

        command.Parameters.Add(new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = id });

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapAuctionDetail(reader);
        }

        return null;
    }

    public async Task<AuctionDetailResponse?> GetByListingIdAsync(int listingId)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            @"SELECT a.id, a.listing_id, l.title, l.description, l.user_id as seller_id,
                     a.starting_price, a.current_price, a.start_time, a.end_time,
                     a.min_bid_increment, a.winner_user_id, a.status
              FROM auctions a
              INNER JOIN listings l ON a.listing_id = l.id
              WHERE a.listing_id = @listingId", connection);

        command.Parameters.Add(new NpgsqlParameter("@listingId", NpgsqlDbType.Integer) { Value = listingId });

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapAuctionDetail(reader);
        }

        return null;
    }

    public async Task<AuctionDetailResponse> PlaceBidWithTransactionAsync(int auctionId, int userId, decimal amount, string? idempotencyKey)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            // 1. SELECT FOR UPDATE - pessimistic row lock
            await using var selectCommand = new NpgsqlCommand(
                @"SELECT id, listing_id, current_price, start_time, end_time, min_bid_increment, status
                  FROM auctions
                  WHERE id = @id
                  FOR UPDATE", connection, transaction);

            selectCommand.Parameters.Add(new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = auctionId });

            await using var reader = await selectCommand.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                throw new KeyNotFoundException($"Auction with ID {auctionId} not found");
            }

            var currentPrice = reader.GetFieldValue<decimal>(reader.GetOrdinal("current_price"));
            var startTime = reader.GetFieldValue<DateTime>(reader.GetOrdinal("start_time"));
            var endTime = reader.GetFieldValue<DateTime>(reader.GetOrdinal("end_time"));
            var minBidIncrement = reader.GetFieldValue<decimal>(reader.GetOrdinal("min_bid_increment"));
            var status = reader.GetFieldValue<string>(reader.GetOrdinal("status"));
            await reader.CloseAsync();

            // 2. Validate status and time
            if (status != "active")
            {
                throw new InvalidOperationException("Auction is not active");
            }

            if (DateTime.UtcNow < startTime)
            {
                throw new InvalidOperationException("Auction has not started yet");
            }

            if (DateTime.UtcNow >= endTime)
            {
                throw new InvalidOperationException("Auction has ended");
            }

            // 3. Validate minimum bid
            var minimumBid = currentPrice + minBidIncrement;
            if (amount < minimumBid)
            {
                throw new InvalidOperationException($"Minimum bid is {minimumBid}");
            }

            // 4. Anti-sniping check
            var newEndTime = endTime;
            if (endTime.AddSeconds(-120) <= DateTime.UtcNow)
            {
                newEndTime = DateTime.UtcNow.AddSeconds(120);
            }

            // 5. Insert bid
            await using var insertBidCommand = new NpgsqlCommand(
                @"INSERT INTO bids (listing_id, user_id, amount, idempotency_key, created_at)
                  SELECT listing_id, @userId, @amount, @idempotencyKey, NOW()
                  FROM auctions WHERE id = @auctionId
                  RETURNING id", connection, transaction);

            insertBidCommand.Parameters.Add(new NpgsqlParameter("@userId", NpgsqlDbType.Integer) { Value = userId });
            insertBidCommand.Parameters.Add(new NpgsqlParameter("@amount", NpgsqlDbType.Numeric) { Value = amount });
            insertBidCommand.Parameters.Add(new NpgsqlParameter("@idempotencyKey", NpgsqlDbType.Varchar) { Value = (object?)idempotencyKey ?? DBNull.Value });
            insertBidCommand.Parameters.Add(new NpgsqlParameter("@auctionId", NpgsqlDbType.Integer) { Value = auctionId });

            await insertBidCommand.ExecuteScalarAsync();

            // 6. Update auction
            await using var updateCommand = new NpgsqlCommand(
                @"UPDATE auctions
                  SET current_price = @amount,
                      winner_user_id = @userId,
                      end_time = @newEndTime
                  WHERE id = @auctionId", connection, transaction);

            updateCommand.Parameters.Add(new NpgsqlParameter("@amount", NpgsqlDbType.Numeric) { Value = amount });
            updateCommand.Parameters.Add(new NpgsqlParameter("@userId", NpgsqlDbType.Integer) { Value = userId });
            updateCommand.Parameters.Add(new NpgsqlParameter("@newEndTime", NpgsqlDbType.TimestampTz) { Value = newEndTime });
            updateCommand.Parameters.Add(new NpgsqlParameter("@auctionId", NpgsqlDbType.Integer) { Value = auctionId });

            await updateCommand.ExecuteNonQueryAsync();

            // 7. Commit transaction
            await transaction.CommitAsync();

            // 8. Return updated auction
            var result = await GetByIdAsync(auctionId);
            return result ?? throw new InvalidOperationException("Auction not found after bid");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<BidHistoryResponse>> GetBidHistoryAsync(int auctionId)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            @"SELECT b.id, b.listing_id, b.user_id,
                     u.first_name || ' ' || u.last_name as user_name,
                     b.amount, b.created_at
              FROM bids b
              INNER JOIN auctions a ON b.listing_id = a.listing_id
              INNER JOIN users u ON b.user_id = u.id
              WHERE a.id = @auctionId
              ORDER BY b.created_at DESC", connection);

        command.Parameters.Add(new NpgsqlParameter("@auctionId", NpgsqlDbType.Integer) { Value = auctionId });

        var bids = new List<BidHistoryResponse>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            bids.Add(new BidHistoryResponse(
                reader.GetFieldValue<int>(reader.GetOrdinal("id")),
                reader.GetFieldValue<int>(reader.GetOrdinal("listing_id")),
                reader.GetFieldValue<int>(reader.GetOrdinal("user_id")),
                reader.GetFieldValue<string>(reader.GetOrdinal("user_name")),
                reader.GetFieldValue<decimal>(reader.GetOrdinal("amount")),
                reader.GetFieldValue<DateTime>(reader.GetOrdinal("created_at"))
            ));
        }

        return bids;
    }

    public async Task<bool> IdempotencyKeyExistsAsync(string key)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            "SELECT COUNT(1) FROM bids WHERE idempotency_key = @key", connection);

        command.Parameters.Add(new NpgsqlParameter("@key", NpgsqlDbType.Varchar) { Value = key });

        var count = (long)(await command.ExecuteScalarAsync())!;
        return count > 0;
    }

    public async Task<long> CountActiveAsync()
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            "SELECT COUNT(1) FROM auctions WHERE status = 'active' AND end_time > NOW()", connection);
        return (long)(await command.ExecuteScalarAsync())!;
    }

    private static AuctionListItemResponse MapAuctionListItem(NpgsqlDataReader reader)
    {
        return new AuctionListItemResponse(
            reader.GetFieldValue<int>(reader.GetOrdinal("id")),
            reader.GetFieldValue<int>(reader.GetOrdinal("listing_id")),
            reader.GetFieldValue<string>(reader.GetOrdinal("title")),
            reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetFieldValue<string>(reader.GetOrdinal("description")),
            reader.GetFieldValue<int>(reader.GetOrdinal("seller_id")),
            reader.GetFieldValue<decimal>(reader.GetOrdinal("starting_price")),
            reader.GetFieldValue<decimal>(reader.GetOrdinal("current_price")),
            reader.GetFieldValue<DateTime>(reader.GetOrdinal("start_time")),
            reader.GetFieldValue<DateTime>(reader.GetOrdinal("end_time")),
            reader.GetFieldValue<decimal>(reader.GetOrdinal("min_bid_increment")),
            reader.GetFieldValue<string>(reader.GetOrdinal("status"))
        );
    }

    private static AuctionDetailResponse MapAuctionDetail(NpgsqlDataReader reader)
    {
        return new AuctionDetailResponse(
            reader.GetFieldValue<int>(reader.GetOrdinal("id")),
            reader.GetFieldValue<int>(reader.GetOrdinal("listing_id")),
            reader.GetFieldValue<string>(reader.GetOrdinal("title")),
            reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetFieldValue<string>(reader.GetOrdinal("description")),
            reader.GetFieldValue<int>(reader.GetOrdinal("seller_id")),
            reader.GetFieldValue<decimal>(reader.GetOrdinal("starting_price")),
            reader.GetFieldValue<decimal>(reader.GetOrdinal("current_price")),
            reader.GetFieldValue<DateTime>(reader.GetOrdinal("start_time")),
            reader.GetFieldValue<DateTime>(reader.GetOrdinal("end_time")),
            reader.GetFieldValue<decimal>(reader.GetOrdinal("min_bid_increment")),
            reader.IsDBNull(reader.GetOrdinal("winner_user_id")) ? null : reader.GetFieldValue<int?>(reader.GetOrdinal("winner_user_id")),
            reader.GetFieldValue<string>(reader.GetOrdinal("status"))
        );
    }
}
