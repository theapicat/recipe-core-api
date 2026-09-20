using Dapper;
using Domain.Ingredients;
using Microsoft.Extensions.Configuration;
using Persistence.Interfaces;
using Persistence.Services;

namespace Persistence.Implementation;

public class UnconfirmedIngredientReader(IConfiguration configuration)
    : DbReader<UnconfirmedIngredient>(configuration.GetConnectionString("DefaultConnection")!),
        IUnconfirmedIngredientReader
{
    public override string? GetByIdQuery { get; } = "SELECT * FROM get_unconfirmed_ingredient_by_id(@Id);";

    public Task<UnconfirmedIngredient?> GetByIdAsync(Guid id) => base.GetByIdAsync<Guid>(id);

    public async Task<List<UnconfirmedIngredient>> GetByUserAsync(Guid userId)
    {
        await using var connection = await OpenConnectionAsync();
        var result = await connection.QueryAsync<UnconfirmedIngredient>(
            "SELECT * FROM get_unconfirmed_ingredient_by_user(@UserId);", new { UserId = userId });
        return result.AsList();
    }

    public async Task<List<UnconfirmedIngredient>> GetByStatusAsync(UnconfirmedIngredientStatus? status)
    {
        await using var connection = await OpenConnectionAsync();
        var result = await connection.QueryAsync<UnconfirmedIngredient>(
            "SELECT * FROM get_all_unconfirmed_ingredient(@Status);", new { Status = status?.ToString() });
        return result.AsList();
    }

    public async Task<int> CountByUserAsync(Guid userId, UnconfirmedIngredientStatus? status = null)
    {
        await using var connection = await OpenConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(
            "SELECT count_unconfirmed_ingredient_by_user(@UserId, @Status);",
            new { UserId = userId, Status = status?.ToString() });
    }
}
