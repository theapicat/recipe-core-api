using Dapper;
using Domain.Recipes;
using Microsoft.Extensions.Configuration;
using Persistence.Interfaces;
using Persistence.Services;

namespace Persistence.Implementation;

public class RecipeReader(IConfiguration configuration)
    : DbConnection(configuration.GetConnectionString("DefaultConnection")!), IRecipeReader
{
    private const string GetByIdSql = """
        SELECT * FROM get_recipe_by_id(@Id, @OwnerUserId);
        SELECT * FROM get_recipe_steps(@Id, @OwnerUserId);
        SELECT * FROM get_recipe_ingredients(@Id, @OwnerUserId);
        """;

    private const string GetNutritionInputSql = """
        SELECT * FROM get_recipe_by_id(@Id, @OwnerUserId);
        SELECT * FROM get_recipe_nutrition_lines(@Id, @OwnerUserId);
        SELECT * FROM get_recipe_nutrition_portions(@Id, @OwnerUserId);
        SELECT * FROM get_recipe_nutrition_values(@Id, @OwnerUserId);
        """;

    public async Task<List<RecipeListItem>> GetListByOwnerAsync(Guid ownerUserId)
    {
        await using var connection = await OpenConnectionAsync();
        var result = await connection.QueryAsync<RecipeListItem>(
            "SELECT * FROM get_all_recipe_list_item(@OwnerUserId);", new { OwnerUserId = ownerUserId });
        return result.AsList();
    }

    public async Task<Recipe?> GetByIdAsync(Guid id, Guid ownerUserId)
    {
        await using var connection = await OpenConnectionAsync();
        using var results = await connection.QueryMultipleAsync(GetByIdSql, new { Id = id, OwnerUserId = ownerUserId });

        var row = await results.ReadSingleOrDefaultAsync<RecipeRow>();
        if (row is null)
            return null;

        var steps = (await results.ReadAsync<RecipeStep>()).ToList();
        var ingredients = (await results.ReadAsync<RecipeIngredient>()).ToList();
        return row.ToRecipe(steps, ingredients);
    }

    public async Task<RecipeNutritionInput?> GetNutritionInputAsync(Guid id, Guid ownerUserId)
    {
        await using var connection = await OpenConnectionAsync();
        using var results = await connection.QueryMultipleAsync(GetNutritionInputSql, new { Id = id, OwnerUserId = ownerUserId });

        var recipe = await results.ReadSingleOrDefaultAsync<RecipeRow>();
        if (recipe is null)
            return null;

        return new RecipeNutritionInput
        {
            RecipeId = recipe.Id,
            Servings = recipe.Servings,
            Lines = (await results.ReadAsync<RecipeNutritionLine>()).ToList(),
            Portions = (await results.ReadAsync<RecipeNutritionPortion>()).ToList(),
            Values = (await results.ReadAsync<RecipeNutritionValue>()).ToList()
        };
    }

    public async Task<int> CountByOwnerAsync(Guid ownerUserId)
    {
        await using var connection = await OpenConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(
            "SELECT count_recipe_by_owner(@OwnerUserId);", new { OwnerUserId = ownerUserId });
    }
}
