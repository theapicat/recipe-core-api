using Npgsql;

namespace Persistence.Services;

public abstract class DbConnection(string connectionString)
{
    protected async Task<NpgsqlConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
