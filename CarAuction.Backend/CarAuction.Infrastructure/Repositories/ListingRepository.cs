using CarAuction.Application.DTOs.Listings;
using CarAuction.Application.Interfaces.Repositories;
using CarAuction.Domain.Entities;
using CarAuction.Infrastructure.Data.Connection;
using Npgsql;
using NpgsqlTypes;

namespace CarAuction.Infrastructure.Repositories;

public class ListingRepository : IListingRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public ListingRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<(List<Listing> Listings, int Total)> GetPaginatedAsync(ListingFilters filters)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        var conditions = new List<string>();
        var parameters = new List<NpgsqlParameter>();

        // Build dynamic SQL based on filters
        if (!string.IsNullOrEmpty(filters.Search))
        {
            conditions.Add("(title ILIKE @search OR description ILIKE @search)");
            parameters.Add(new NpgsqlParameter("@search", NpgsqlDbType.Varchar) { Value = $"%{filters.Search}%" });
        }

        if (filters.MinPrice.HasValue)
        {
            conditions.Add("price >= @minPrice");
            parameters.Add(new NpgsqlParameter("@minPrice", NpgsqlDbType.Numeric) { Value = filters.MinPrice.Value });
        }

        if (filters.MaxPrice.HasValue)
        {
            conditions.Add("price <= @maxPrice");
            parameters.Add(new NpgsqlParameter("@maxPrice", NpgsqlDbType.Numeric) { Value = filters.MaxPrice.Value });
        }

        if (!string.IsNullOrEmpty(filters.Status))
        {
            conditions.Add("status = @status");
            parameters.Add(new NpgsqlParameter("@status", NpgsqlDbType.Varchar) { Value = filters.Status });
        }

        if (filters.IsAuction.HasValue)
        {
            conditions.Add("is_auction = @isAuction");
            parameters.Add(new NpgsqlParameter("@isAuction", NpgsqlDbType.Boolean) { Value = filters.IsAuction.Value });
        }

        var whereClause = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";

        // Get total count
        await using var countCommand = new NpgsqlCommand($"SELECT COUNT(1) FROM listings {whereClause}", connection);
        foreach (var param in parameters)
        {
            countCommand.Parameters.Add(param);
        }
        var total = (long)(await countCommand.ExecuteScalarAsync())!;

        // Get paginated listings
        var offset = (filters.Page - 1) * filters.Limit;
        await using var command = new NpgsqlCommand(
            $"SELECT id, user_id, title, description, price, is_auction, status, created_at, updated_at " +
            $"FROM listings {whereClause} ORDER BY created_at DESC LIMIT @limit OFFSET @offset", connection);

        foreach (var param in parameters)
        {
            command.Parameters.Add(param);
        }
        command.Parameters.Add(new NpgsqlParameter("@limit", NpgsqlDbType.Integer) { Value = filters.Limit });
        command.Parameters.Add(new NpgsqlParameter("@offset", NpgsqlDbType.Integer) { Value = offset });

        var listings = new List<Listing>();
        await using (var reader = await command.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                listings.Add(MapListing(reader));
            }
        }

        await PopulateImagesForListingsAsync(listings, connection);

        return (listings, (int)total);
    }

    public async Task<Listing?> GetByIdAsync(int id)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            @"SELECT id, user_id, title, description, price, is_auction, status, created_at, updated_at 
              FROM listings WHERE id = @id", connection);

        command.Parameters.Add(new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = id });

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapListing(reader);
        }

        return null;
    }

    public async Task<Listing?> GetByIdWithImagesAsync(int id)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            @"SELECT id, user_id, title, description, price, is_auction, status, created_at, updated_at 
              FROM listings WHERE id = @id", connection);

        command.Parameters.Add(new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = id });

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var listing = MapListing(reader);

            // Get images
            listing.Images = new List<Image>(await GetImagesByListingIdAsync(id));

            return listing;
        }

        return null;
    }

    public async Task<int> CreateAsync(Listing listing)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            @"INSERT INTO listings (user_id, title, description, price, is_auction, status, created_at) 
              VALUES (@userId, @title, @description, @price, @isAuction, @status, @createdAt) 
              RETURNING id", connection);

        command.Parameters.Add(new NpgsqlParameter("@userId", NpgsqlDbType.Integer) { Value = listing.UserId });
        command.Parameters.Add(new NpgsqlParameter("@title", NpgsqlDbType.Varchar) { Value = listing.Title });
        command.Parameters.Add(new NpgsqlParameter("@description", NpgsqlDbType.Text) { Value = (object?)listing.Description ?? DBNull.Value });
        command.Parameters.Add(new NpgsqlParameter("@price", NpgsqlDbType.Numeric) { Value = listing.Price });
        command.Parameters.Add(new NpgsqlParameter("@isAuction", NpgsqlDbType.Boolean) { Value = listing.IsAuction });
        command.Parameters.Add(new NpgsqlParameter("@status", NpgsqlDbType.Varchar) { Value = listing.Status });
        command.Parameters.Add(new NpgsqlParameter("@createdAt", NpgsqlDbType.TimestampTz) { Value = listing.CreatedAt });

        var id = (int)(await command.ExecuteScalarAsync())!;
        return id;
    }

    public async Task CreateAuctionAsync(Auction auction)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            @"INSERT INTO auctions (listing_id, starting_price, current_price, start_time, end_time, min_bid_increment, status) 
              VALUES (@listingId, @startingPrice, @currentPrice, @startTime, @endTime, @minBidIncrement, @status)", connection);

        command.Parameters.Add(new NpgsqlParameter("@listingId", NpgsqlDbType.Integer) { Value = auction.ListingId });
        command.Parameters.Add(new NpgsqlParameter("@startingPrice", NpgsqlDbType.Numeric) { Value = auction.StartingPrice });
        command.Parameters.Add(new NpgsqlParameter("@currentPrice", NpgsqlDbType.Numeric) { Value = auction.CurrentPrice });
        command.Parameters.Add(new NpgsqlParameter("@startTime", NpgsqlDbType.TimestampTz) { Value = auction.StartTime });
        command.Parameters.Add(new NpgsqlParameter("@endTime", NpgsqlDbType.TimestampTz) { Value = auction.EndTime });
        command.Parameters.Add(new NpgsqlParameter("@minBidIncrement", NpgsqlDbType.Numeric) { Value = auction.MinBidIncrement });
        command.Parameters.Add(new NpgsqlParameter("@status", NpgsqlDbType.Varchar) { Value = auction.Status });

        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateAsync(Listing listing)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            @"UPDATE listings SET title = @title, description = @description, price = @price, updated_at = @updatedAt 
              WHERE id = @id", connection);

        command.Parameters.Add(new NpgsqlParameter("@title", NpgsqlDbType.Varchar) { Value = listing.Title });
        command.Parameters.Add(new NpgsqlParameter("@description", NpgsqlDbType.Text) { Value = (object?)listing.Description ?? DBNull.Value });
        command.Parameters.Add(new NpgsqlParameter("@price", NpgsqlDbType.Numeric) { Value = listing.Price });
        command.Parameters.Add(new NpgsqlParameter("@updatedAt", NpgsqlDbType.TimestampTz) { Value = listing.UpdatedAt!.Value });
        command.Parameters.Add(new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = listing.Id });

        await command.ExecuteNonQueryAsync();
    }

    public async Task SoftDeleteAsync(int id)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            @"UPDATE listings SET status = 'cancelled', updated_at = @updatedAt WHERE id = @id", connection);

        command.Parameters.Add(new NpgsqlParameter("@updatedAt", NpgsqlDbType.TimestampTz) { Value = DateTime.UtcNow });
        command.Parameters.Add(new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = id });

        await command.ExecuteNonQueryAsync();
    }

    public async Task<bool> IsOwnerAsync(int id, int userId)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            "SELECT COUNT(1) FROM listings WHERE id = @id AND user_id = @userId", connection);

        command.Parameters.Add(new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = id });
        command.Parameters.Add(new NpgsqlParameter("@userId", NpgsqlDbType.Integer) { Value = userId });

        var count = (long)(await command.ExecuteScalarAsync())!;
        return count > 0;
    }

    public async Task<List<Listing>> GetByUserIdAsync(int userId)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            @"SELECT id, user_id, title, description, price, is_auction, status, created_at, updated_at 
              FROM listings WHERE user_id = @userId ORDER BY created_at DESC", connection);

        command.Parameters.Add(new NpgsqlParameter("@userId", NpgsqlDbType.Integer) { Value = userId });

        var listings = new List<Listing>();
        await using (var reader = await command.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                listings.Add(MapListing(reader));
            }
        }

        await PopulateImagesForListingsAsync(listings, connection);

        return listings;
    }

    private static async Task PopulateImagesForListingsAsync(List<Listing> listings, NpgsqlConnection connection)
    {
        if (listings.Count == 0) return;

        var listingIds = listings.Select(l => l.Id).ToList();
        await using var imgCommand = new NpgsqlCommand(
            "SELECT id, listing_id, file_name, mime_type, uploaded_at FROM images WHERE listing_id = ANY(@ids) ORDER BY id ASC", connection);
        imgCommand.Parameters.Add(new NpgsqlParameter("@ids", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = listingIds.ToArray() });
        await using var imgReader = await imgCommand.ExecuteReaderAsync();
        var imagesByListing = new Dictionary<int, List<Image>>();
        while (await imgReader.ReadAsync())
        {
            var listingId = imgReader.GetInt32(1);
            if (!imagesByListing.ContainsKey(listingId))
            {
                imagesByListing[listingId] = new List<Image>();
            }
            imagesByListing[listingId].Add(new Image
            {
                Id = imgReader.GetInt32(0),
                ListingId = listingId,
                FileName = imgReader.IsDBNull(2) ? null : imgReader.GetString(2),
                MimeType = imgReader.IsDBNull(3) ? null : imgReader.GetString(3),
                UploadedAt = imgReader.GetDateTime(4)
            });
        }
        foreach (var listing in listings)
        {
            if (imagesByListing.TryGetValue(listing.Id, out var imgs))
            {
                listing.Images = imgs;
            }
        }
    }

    public async Task<List<Image>> GetImagesByListingIdAsync(int listingId)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            "SELECT id, listing_id, file_name, mime_type, uploaded_at FROM images WHERE listing_id = @listingId", connection);

        command.Parameters.Add(new NpgsqlParameter("@listingId", NpgsqlDbType.Integer) { Value = listingId });

        var images = new List<Image>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            images.Add(new Image
            {
                Id = reader.GetFieldValue<int>(reader.GetOrdinal("id")),
                ListingId = reader.GetFieldValue<int>(reader.GetOrdinal("listing_id")),
                FileName = reader.IsDBNull(reader.GetOrdinal("file_name")) ? null : reader.GetFieldValue<string>(reader.GetOrdinal("file_name")),
                MimeType = reader.IsDBNull(reader.GetOrdinal("mime_type")) ? null : reader.GetFieldValue<string>(reader.GetOrdinal("mime_type")),
                UploadedAt = reader.GetFieldValue<DateTime>(reader.GetOrdinal("uploaded_at"))
            });
        }

        return images;
    }

    public async Task AddImageAsync(Image image)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            @"INSERT INTO images (listing_id, image_data, file_name, mime_type, uploaded_at) 
              VALUES (@listingId, @imageData, @fileName, @mimeType, @uploadedAt) 
              RETURNING id", connection);

        command.Parameters.Add(new NpgsqlParameter("@listingId", NpgsqlDbType.Integer) { Value = image.ListingId });
        command.Parameters.Add(new NpgsqlParameter("@imageData", NpgsqlDbType.Bytea) { Value = image.ImageData });
        command.Parameters.Add(new NpgsqlParameter("@fileName", NpgsqlDbType.Varchar) { Value = (object?)image.FileName ?? DBNull.Value });
        command.Parameters.Add(new NpgsqlParameter("@mimeType", NpgsqlDbType.Varchar) { Value = (object?)image.MimeType ?? DBNull.Value });
        command.Parameters.Add(new NpgsqlParameter("@uploadedAt", NpgsqlDbType.TimestampTz) { Value = image.UploadedAt });

        image.Id = (int)(await command.ExecuteScalarAsync())!;
    }

    private static Listing MapListing(NpgsqlDataReader reader)
    {
        return new Listing
        {
            Id = reader.GetFieldValue<int>(reader.GetOrdinal("id")),
            UserId = reader.GetFieldValue<int>(reader.GetOrdinal("user_id")),
            Title = reader.GetFieldValue<string>(reader.GetOrdinal("title")),
            Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetFieldValue<string>(reader.GetOrdinal("description")),
            Price = reader.GetFieldValue<decimal>(reader.GetOrdinal("price")),
            IsAuction = reader.GetFieldValue<bool>(reader.GetOrdinal("is_auction")),
            Status = reader.GetFieldValue<string>(reader.GetOrdinal("status")),
            CreatedAt = reader.GetFieldValue<DateTime>(reader.GetOrdinal("created_at")),
            UpdatedAt = reader.IsDBNull(reader.GetOrdinal("updated_at")) ? null : reader.GetFieldValue<DateTime?>(reader.GetOrdinal("updated_at"))
        };
    }

    public async Task<long> CountAsync()
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand("SELECT COUNT(1) FROM listings", connection);
        return (long)(await command.ExecuteScalarAsync())!;
    }

    public async Task<long> CountByStatusAsync(string status)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            "SELECT COUNT(1) FROM listings WHERE status = @status", connection);
        command.Parameters.Add(new NpgsqlParameter("@status", NpgsqlDbType.Varchar) { Value = status });
        return (long)(await command.ExecuteScalarAsync())!;
    }

    public async Task<List<Listing>> GetRecentAsync(int limit)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            @"SELECT id, user_id, title, description, price, is_auction, status, created_at, updated_at 
              FROM listings ORDER BY created_at DESC LIMIT @limit", connection);
        command.Parameters.Add(new NpgsqlParameter("@limit", NpgsqlDbType.Integer) { Value = limit });

        var listings = new List<Listing>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            listings.Add(MapListing(reader));
        }
        return listings;
    }
}
