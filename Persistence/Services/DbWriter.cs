using Dapper;
using Persistence.Exceptions;

namespace Persistence.Services;

public abstract class DbWriter<T>(string connectionString) : DbConnection(connectionString)
{
    public virtual string? InsertCommand { get; } = null;
    public virtual string? UpdateCommand { get; } = null;
    public virtual string? DeleteCommand { get; } = null;

    public virtual async Task AddAsync(T entity)
    {
        if (InsertCommand is null)
            throw new DbCommandMissingException<T>(nameof(InsertCommand));

        await using var connection = await OpenConnectionAsync();
        await connection.ExecuteAsync(InsertCommand, entity);
    }

    public virtual async Task UpdateAsync(T entity)
    {
        if (UpdateCommand is null)
            throw new DbCommandMissingException<T>(nameof(UpdateCommand));

        await using var connection = await OpenConnectionAsync();
        await connection.ExecuteAsync(UpdateCommand, entity);
    }

    public virtual async Task<int> DeleteAsync<TId>(TId id)
    {
        if (DeleteCommand is null)
            throw new DbCommandMissingException<T>(nameof(DeleteCommand));

        await using var connection = await OpenConnectionAsync();
        return await connection.ExecuteAsync(DeleteCommand, new { Id = id });
    }
}
