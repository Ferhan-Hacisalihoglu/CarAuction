using CarAuction.Application.Interfaces.Repositories;
using CarAuction.Domain.Entities;
using CarAuction.Infrastructure.Data.Connection;
using Npgsql;
using NpgsqlTypes;

namespace CarAuction.Infrastructure.Repositories;

public class ImageRepository : IImageRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public ImageRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> CreateAsync(Image image)
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

        var id = (int)(await command.ExecuteScalarAsync())!;
        return id;
    }

    public async Task<Image?> GetByIdAsync(int id)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            "SELECT id, listing_id, image_data, file_name, mime_type, uploaded_at FROM images WHERE id = @id", connection);

        command.Parameters.Add(new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = id });

        await using var reader = await command.ExecuteReaderAsync(System.Data.CommandBehavior.SequentialAccess);
        if (await reader.ReadAsync())
        {
            var imgId = reader.GetInt32(0);
            var listingId = reader.GetInt32(1);

            // Read bytea sequentially using GetBytes to stream without LOH memory bloat
            var length = reader.GetBytes(2, 0, null, 0, 0);
            var buffer = new byte[length];
            reader.GetBytes(2, 0, buffer, 0, (int)length);

            var fileName = reader.IsDBNull(3) ? null : reader.GetString(3);
            var mimeType = reader.IsDBNull(4) ? null : reader.GetString(4);
            var uploadedAt = reader.GetDateTime(5);

            return new Image
            {
                Id = imgId,
                ListingId = listingId,
                ImageData = buffer,
                FileName = fileName,
                MimeType = mimeType,
                UploadedAt = uploadedAt
            };
        }

        return null;
    }

    public async Task DeleteAsync(int id)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            "DELETE FROM images WHERE id = @id", connection);

        command.Parameters.Add(new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = id });

        await command.ExecuteNonQueryAsync();
    }

    public async Task<Listing?> GetListingByIdAsync(int id)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            "SELECT id, user_id, title, description, price, is_auction, status, created_at, updated_at FROM listings WHERE id = @id", connection);

        command.Parameters.Add(new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = id });

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
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

        return null;
    }

    public async Task<bool> IsUserAdminAsync(int userId)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var command = new NpgsqlCommand(
            @"SELECT COUNT(1) FROM users u 
              INNER JOIN roles r ON u.role_id = r.id 
              WHERE u.id = @userId AND r.name = 'Admin'", connection);

        command.Parameters.Add(new NpgsqlParameter("@userId", NpgsqlDbType.Integer) { Value = userId });

        var count = (long)(await command.ExecuteScalarAsync())!;
        return count > 0;
    }
}
