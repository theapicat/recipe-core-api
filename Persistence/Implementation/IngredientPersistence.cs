using System.Data;
using Dapper;
using Domain.Ingredients;

namespace Persistence.Implementation;

// Delt SQL-logikk for å skrive en ingrediens med barn. Kalles innenfor en transaksjon eieren har åpnet
// (IngredientWriter for vanlig opprett/oppdater, UnconfirmedIngredientWriter når en godkjenning skal
// opprette ingrediensen og avgjøre forespørselen atomisk).
internal static class IngredientPersistence
{
    private const string InsertIngredientSql = """
        SELECT insert_ingredient(@Id, @Name, @CategoryId, @PrimaryUnitTypeId, @DefaultUnitId, @EnergyKcal, @EnergyKj,
                                 @EdiblePartPercent, @SourceId, @SourceUrl, @VariantOfIngredientId, @IsVerified);
        """;

    private const string UpdateIngredientSql = """
        SELECT update_ingredient(@Id, @Name, @CategoryId, @PrimaryUnitTypeId, @DefaultUnitId, @EnergyKcal, @EnergyKj,
                                 @EdiblePartPercent, @SourceId, @SourceUrl, @VariantOfIngredientId, @IsVerified);
        """;

    public static async Task InsertAsync(IDbConnection connection, IDbTransaction transaction, Ingredient ingredient)
    {
        await connection.ExecuteAsync(InsertIngredientSql, ToParameters(ingredient), transaction);
        await InsertChildrenAsync(connection, transaction, ingredient);
    }

    // Barna erstattes (slett + sett inn på nytt) - enklere enn å diffe, og alt skjer i samme transaksjon.
    public static async Task UpdateAsync(IDbConnection connection, IDbTransaction transaction, Ingredient ingredient)
    {
        await connection.ExecuteAsync(UpdateIngredientSql, ToParameters(ingredient), transaction);

        var id = new { ingredient.Id };
        await connection.ExecuteAsync("SELECT delete_ingredient_allergens(@Id);", id, transaction);
        await connection.ExecuteAsync("SELECT delete_ingredient_search_keywords(@Id);", id, transaction);
        await connection.ExecuteAsync("SELECT delete_ingredient_nutrient_values(@Id);", id, transaction);
        await connection.ExecuteAsync("SELECT delete_ingredient_portions(@Id);", id, transaction);

        await InsertChildrenAsync(connection, transaction, ingredient);
    }

    private static async Task InsertChildrenAsync(IDbConnection connection, IDbTransaction transaction, Ingredient ingredient)
    {
        var allergens = ingredient.AllergenIds
            .Select(allergenId => new { IngredientId = ingredient.Id, AllergenId = allergenId })
            .ToList();
        if (allergens.Count > 0)
            await connection.ExecuteAsync("SELECT insert_ingredient_allergen(@IngredientId, @AllergenId);", allergens, transaction);

        var keywords = ingredient.SearchKeywordIds
            .Select(keywordId => new { IngredientId = ingredient.Id, SearchKeywordId = keywordId })
            .ToList();
        if (keywords.Count > 0)
            await connection.ExecuteAsync("SELECT insert_ingredient_search_keyword(@IngredientId, @SearchKeywordId);", keywords, transaction);

        var nutrientValues = ingredient.NutrientValues
            .Select(v => new { v.Id, IngredientId = ingredient.Id, v.NutrientDefinitionId, v.Quantity, v.SourceId })
            .ToList();
        if (nutrientValues.Count > 0)
            await connection.ExecuteAsync(
                "SELECT insert_ingredient_nutrient_value(@Id, @IngredientId, @NutrientDefinitionId, @Quantity, @SourceId);",
                nutrientValues, transaction);

        var portions = ingredient.Portions
            .Select(p => new { p.Id, IngredientId = ingredient.Id, p.UnitId, p.GramsPerPortion })
            .ToList();
        if (portions.Count > 0)
            await connection.ExecuteAsync(
                "SELECT insert_ingredient_portion(@Id, @IngredientId, @UnitId, @GramsPerPortion);",
                portions, transaction);
    }

    private static object ToParameters(Ingredient i) => new
    {
        i.Id, i.Name, i.CategoryId, i.PrimaryUnitTypeId, i.DefaultUnitId, i.EnergyKcal, i.EnergyKj,
        i.EdiblePartPercent, i.SourceId, i.SourceUrl, i.VariantOfIngredientId, i.IsVerified
    };
}
