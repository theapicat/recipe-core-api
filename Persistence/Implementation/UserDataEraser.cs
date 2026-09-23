using Dapper;
using Microsoft.Extensions.Configuration;
using Persistence.Interfaces;
using Persistence.Services;

namespace Persistence.Implementation;

// Rekkefølgen er bevisst: oppskrifter (og linjene deres) må slettes før de ubekreftede ingrediensene de kan peke på -
// recipe_ingredient.unconfirmed_ingredient_id har ON DELETE RESTRICT.
public class UserDataEraser(IConfiguration configuration)
    : DbConnection(configuration.GetConnectionString("DefaultConnection")!), IUserDataEraser
{
    public async Task<UserDataDeletionResult> DeleteAllForUserAsync(Guid userId)
    {
        await using var connection = await OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var recipesDeleted = await connection.ExecuteScalarAsync<int>(
            "SELECT delete_recipes_by_owner(@UserId);", new { UserId = userId }, transaction);
        var unconfirmedIngredientsDeleted = await connection.ExecuteScalarAsync<int>(
            "SELECT delete_unconfirmed_ingredients_by_user(@UserId);", new { UserId = userId }, transaction);

        await transaction.CommitAsync();
        return new UserDataDeletionResult(recipesDeleted, unconfirmedIngredientsDeleted);
    }
}
