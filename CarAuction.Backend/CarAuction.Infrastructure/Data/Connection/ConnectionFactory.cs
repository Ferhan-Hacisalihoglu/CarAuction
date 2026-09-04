using Npgsql;

namespace CarAuction.Infrastructure.Data.Connection;

public interface IConnectionFactory
{
    Task<NpgsqlConnection> CreateConnectionAsync();
    Task<bool> CanConnectAsync();
}

public class ConnectionFactory : IConnectionFactory
{
    private readonly NpgsqlDataSource _dataSource;

    public ConnectionFactory(string connectionString)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        _dataSource = dataSourceBuilder.Build();
    }

    public async Task<NpgsqlConnection> CreateConnectionAsync()
    {
        return await _dataSource.OpenConnectionAsync();
    }

    public async Task<bool> CanConnectAsync()
    {
        try
        {
            await using var conn = await _dataSource.OpenConnectionAsync();
            await using var cmd = new NpgsqlCommand("SELECT 1", conn);
            await cmd.ExecuteScalarAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
