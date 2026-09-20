using Dapper;
using Domain.Ingredients;
using Microsoft.Extensions.Configuration;
using Persistence.Services;

namespace Persistence.Implementation;

// Full ingrediens med barn (allergener, nøkkelord, næringsverdier, porsjoner), hentet i ett kall til databasen.
public class IngredientReader(IConfiguration configuration)
    : DbReader<Ingredient>(configuration.GetConnectionString("DefaultConnection")!)
{
    private const string GetByIdSql = """
        SELECT * FROM get_ingredient_by_id(@Id);
        SELECT * FROM get_ingredient_allergen_ids(@Id);
        SELECT * FROM get_ingredient_search_keyword_ids(@Id);
        SELECT * FROM get_ingredient_nutrient_values(@Id);
        SELECT * FROM get_ingredient_portions(@Id);
        """;

    public override async Task<Ingredient?> GetByIdAsync<TId>(TId id)
    {
        await using var connection = await OpenConnectionAsync();
        using var results = await connection.QueryMultipleAsync(GetByIdSql, new { Id = id });

        var ingredient = await results.ReadSingleOrDefaultAsync<Ingredient>();
        if (ingredient is null)
            return null;

        ingredient.AllergenIds = (await results.ReadAsync<Guid>()).ToList();
        ingredient.SearchKeywordIds = (await results.ReadAsync<Guid>()).ToList();
        ingredient.NutrientValues = (await results.ReadAsync<IngredientNutrientValue>()).ToList();
        ingredient.Portions = (await results.ReadAsync<IngredientPortion>()).ToList();
        return ingredient;
    }
}
