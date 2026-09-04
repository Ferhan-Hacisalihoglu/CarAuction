using Npgsql;

namespace CarAuction.Infrastructure.Data.Connection;

public interface IConnectionFactory
{
    Task<NpgsqlConnection> CreateConnectionAsync();
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
}
