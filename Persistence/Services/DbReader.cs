using Dapper;
using Persistence.Exceptions;

namespace Persistence.Services;

public abstract class DbReader<T>(string connectionString) : DbConnection(connectionString)
{
    public virtual string? GetAllQuery { get; } = null;
    public virtual string? GetByIdQuery { get; } = null;

    public virtual async Task<List<T>> GetAllAsync()
    {
        if (GetAllQuery is null)
            throw new DbQueryMissingException<T>(nameof(GetAllQuery));

        await using var connection = await OpenConnectionAsync();
        var result = await connection.QueryAsync<T>(GetAllQuery);
        return result.AsList();
    }

    public virtual async Task<T?> GetByIdAsync<TId>(TId id)
    {
        if (GetByIdQuery is null)
            throw new DbQueryMissingException<T>(nameof(GetByIdQuery));

        await using var connection = await OpenConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<T>(GetByIdQuery, new { Id = id });
    }
}
