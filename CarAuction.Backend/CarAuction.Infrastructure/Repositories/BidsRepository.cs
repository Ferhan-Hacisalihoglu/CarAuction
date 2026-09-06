using CarAuction.Application.DTOs.Bids;
using CarAuction.Application.Interfaces.Repositories;
using CarAuction.Infrastructure.Data.Connection;
using Npgsql;
using NpgsqlTypes;

namespace CarAuction.Infrastructure.Repositories;

public class BidsRepository : IBidsRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public BidsRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<OfferResponse> MakeOfferAsync(int listingId, int userId, decimal amount)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            @"INSERT INTO bids (listing_id, user_id, amount, created_at)
              VALUES (@listingId, @userId, @amount, NOW())
              RETURNING id, created_at", connection);

        command.Parameters.Add(new NpgsqlParameter("@listingId", NpgsqlDbType.Integer) { Value = listingId });
        command.Parameters.Add(new NpgsqlParameter("@userId", NpgsqlDbType.Integer) { Value = userId });
        command.Parameters.Add(new NpgsqlParameter("@amount", NpgsqlDbType.Numeric) { Value = amount });

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            throw new InvalidOperationException("Failed to create offer");
        }

        var id = reader.GetFieldValue<int>(reader.GetOrdinal("id"));
        var createdAt = reader.GetFieldValue<DateTime>(reader.GetOrdinal("created_at"));

        // Get listing and user info
        var offer = await GetOfferByIdAsync(connection, id);
        if (offer == null)
        {
            throw new InvalidOperationException("Failed to retrieve offer after creation");
        }

        return offer;
    }

    public async Task<List<OfferResponse>> GetOffersByListingIdAsync(int listingId)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            @"SELECT b.id, b.listing_id, l.title as listing_title,
                     b.user_id, u.first_name || ' ' || u.last_name as user_name,
                     b.amount, b.created_at
              FROM bids b
              INNER JOIN listings l ON b.listing_id = l.id
              INNER JOIN users u ON b.user_id = u.id
              WHERE b.listing_id = @listingId
              ORDER BY b.created_at DESC", connection);

        command.Parameters.Add(new NpgsqlParameter("@listingId", NpgsqlDbType.Integer) { Value = listingId });

        var offers = new List<OfferResponse>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            offers.Add(MapOffer(reader));
        }

        return offers;
    }

    public async Task<List<MyBidResponse>> GetMyBidsAsync(int userId)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            @"SELECT b.id, b.listing_id, l.title as listing_title,
                     l.price as listing_price, l.status as listing_status,
                     b.amount, b.created_at
              FROM bids b
              INNER JOIN listings l ON b.listing_id = l.id
              WHERE b.user_id = @userId
              ORDER BY b.created_at DESC", connection);

        command.Parameters.Add(new NpgsqlParameter("@userId", NpgsqlDbType.Integer) { Value = userId });

        var bids = new List<MyBidResponse>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            bids.Add(new MyBidResponse(
                reader.GetFieldValue<int>(reader.GetOrdinal("id")),
                reader.GetFieldValue<int>(reader.GetOrdinal("listing_id")),
                reader.GetFieldValue<string>(reader.GetOrdinal("listing_title")),
                reader.GetFieldValue<decimal>(reader.GetOrdinal("listing_price")),
                reader.GetFieldValue<string>(reader.GetOrdinal("listing_status")),
                reader.GetFieldValue<decimal>(reader.GetOrdinal("amount")),
                reader.GetFieldValue<DateTime>(reader.GetOrdinal("created_at"))
            ));
        }

        return bids;
    }

    public async Task<bool> IsListingOwnerAsync(int listingId, int userId)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            "SELECT COUNT(1) FROM listings WHERE id = @listingId AND user_id = @userId", connection);

        command.Parameters.Add(new NpgsqlParameter("@listingId", NpgsqlDbType.Integer) { Value = listingId });
        command.Parameters.Add(new NpgsqlParameter("@userId", NpgsqlDbType.Integer) { Value = userId });

        var count = (long)(await command.ExecuteScalarAsync())!;
        return count > 0;
    }

    private static async Task<OfferResponse?> GetOfferByIdAsync(NpgsqlConnection connection, int id)
    {
        await using var command = new NpgsqlCommand(
            @"SELECT b.id, b.listing_id, l.title as listing_title,
                     b.user_id, u.first_name || ' ' || u.last_name as user_name,
                     b.amount, b.created_at
              FROM bids b
              INNER JOIN listings l ON b.listing_id = l.id
              INNER JOIN users u ON b.user_id = u.id
              WHERE b.id = @id", connection);

        command.Parameters.Add(new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = id });

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapOffer(reader);
        }

        return null;
    }

    private static OfferResponse MapOffer(NpgsqlDataReader reader)
    {
        return new OfferResponse(
            reader.GetFieldValue<int>(reader.GetOrdinal("id")),
            reader.GetFieldValue<int>(reader.GetOrdinal("listing_id")),
            reader.GetFieldValue<string>(reader.GetOrdinal("listing_title")),
            reader.GetFieldValue<int>(reader.GetOrdinal("user_id")),
            reader.GetFieldValue<string>(reader.GetOrdinal("user_name")),
            reader.GetFieldValue<decimal>(reader.GetOrdinal("amount")),
            reader.GetFieldValue<DateTime>(reader.GetOrdinal("created_at"))
        );
    }

    public async Task<long> CountRecentAsync(TimeSpan period)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            "SELECT COUNT(1) FROM bids WHERE created_at > NOW() - @period", connection);
        command.Parameters.Add(new NpgsqlParameter("@period", NpgsqlDbType.Interval) { Value = period });
        return (long)(await command.ExecuteScalarAsync())!;
    }

    public async Task<List<BidInfo>> GetRecentAsync(int limit)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            @"SELECT b.id, b.listing_id, l.title as listing_title, b.amount, b.created_at
              FROM bids b
              INNER JOIN listings l ON b.listing_id = l.id
              ORDER BY b.created_at DESC LIMIT @limit", connection);
        command.Parameters.Add(new NpgsqlParameter("@limit", NpgsqlDbType.Integer) { Value = limit });

        var bids = new List<BidInfo>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            bids.Add(new BidInfo(
                reader.GetFieldValue<int>(reader.GetOrdinal("id")),
                reader.GetFieldValue<int>(reader.GetOrdinal("listing_id")),
                reader.GetFieldValue<string>(reader.GetOrdinal("listing_title")),
                reader.GetFieldValue<decimal>(reader.GetOrdinal("amount")),
                reader.GetFieldValue<DateTime>(reader.GetOrdinal("created_at"))
            ));
        }
        return bids;
    }

    public async Task UpdateBidStatusAsync(int bidId, string status)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            "UPDATE bids SET status = @status WHERE id = @id", connection);
        command.Parameters.Add(new NpgsqlParameter("@status", NpgsqlDbType.Varchar) { Value = status });
        command.Parameters.Add(new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = bidId });
        await command.ExecuteNonQueryAsync();
    }

    public async Task<int> GetListingIdByBidIdAsync(int bidId)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            "SELECT listing_id FROM bids WHERE id = @id", connection);
        command.Parameters.Add(new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = bidId });
        var result = await command.ExecuteScalarAsync();
        return result == null || result == DBNull.Value ? 0 : (int)result;
    }

    public async Task<OfferResponse?> GetOfferByIdAsync(int id)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        return await GetOfferByIdAsync(connection, id);
    }

    public async Task<string?> GetBidStatusAsync(int bidId)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            "SELECT status FROM bids WHERE id = @id", connection);
        command.Parameters.Add(new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = bidId });
        var result = await command.ExecuteScalarAsync();
        return result == null || result == DBNull.Value ? null : (string)result;
    }
}
