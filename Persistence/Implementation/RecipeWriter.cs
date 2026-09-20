using System.Data;
using Dapper;
using Domain.Recipes;
using Microsoft.Extensions.Configuration;
using Persistence.Interfaces;
using Persistence.Services;

namespace Persistence.Implementation;

// Oppskriften og barna skrives i én transaksjon. Oppdatering erstatter barna (slett + sett inn på nytt) - enklere enn å diffe.
public class RecipeWriter(IConfiguration configuration)
    : DbConnection(configuration.GetConnectionString("DefaultConnection")!), IRecipeWriter
{
    // Enum sendes som tekst (kolonnen er text med CHECK) - Dapper sender ellers enum som heltall.
    private const string InsertRecipeSql = """
        SELECT insert_recipe(@Id, @OwnerUserId, @Title, @Description, @CategoryId, @CookTimeMinutes, @Servings, @ImageUrl,
                             @ImageAttribution, @IsFavorite, @SourceType, @SourceReference, @SourceUrl,
                             @SourceIsEditedFromSource, @CreatedAt, @UpdatedAt);
        """;

    private const string UpdateRecipeSql = """
        SELECT update_recipe(@Id, @OwnerUserId, @Title, @Description, @CategoryId, @CookTimeMinutes, @Servings, @ImageUrl,
                             @ImageAttribution, @SourceReference, @SourceIsEditedFromSource, @UpdatedAt);
        """;

    public async Task AddAsync(Recipe recipe)
    {
        await using var connection = await OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        await connection.ExecuteAsync(InsertRecipeSql, ToParameters(recipe), transaction);
        await InsertChildrenAsync(connection, transaction, recipe);

        await transaction.CommitAsync();
    }

    public async Task<bool> UpdateAsync(Recipe recipe)
    {
        await using var connection = await OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var affected = await connection.ExecuteScalarAsync<int>(UpdateRecipeSql, ToParameters(recipe), transaction);

        // Finnes ikke eller er ikke din: rull tilbake uten å røre barna.
        if (affected == 0)
        {
            await transaction.RollbackAsync();
            return false;
        }

        var id = new { recipe.Id };
        await connection.ExecuteAsync("SELECT delete_recipe_steps(@Id);", id, transaction);
        await connection.ExecuteAsync("SELECT delete_recipe_ingredients(@Id);", id, transaction);
        await InsertChildrenAsync(connection, transaction, recipe);

        await transaction.CommitAsync();
        return true;
    }

    public async Task<bool> SetFavoriteAsync(Guid id, Guid ownerUserId, bool isFavorite)
    {
        await using var connection = await OpenConnectionAsync();
        var affected = await connection.ExecuteScalarAsync<int>(
            "SELECT set_recipe_favorite(@Id, @OwnerUserId, @IsFavorite);",
            new { Id = id, OwnerUserId = ownerUserId, IsFavorite = isFavorite });
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid ownerUserId)
    {
        await using var connection = await OpenConnectionAsync();
        var affected = await connection.ExecuteScalarAsync<int>(
            "SELECT delete_recipe(@Id, @OwnerUserId);", new { Id = id, OwnerUserId = ownerUserId });
        return affected > 0;
    }

    private static async Task InsertChildrenAsync(IDbConnection connection, IDbTransaction transaction, Recipe recipe)
    {
        var steps = recipe.Steps
            .Select(s => new { s.Id, RecipeId = recipe.Id, s.StepNumber, s.Description, s.TimerMinutes })
            .ToList();
        if (steps.Count > 0)
            await connection.ExecuteAsync(
                "SELECT insert_recipe_step(@Id, @RecipeId, @StepNumber, @Description, @TimerMinutes);", steps, transaction);

        var ingredients = recipe.Ingredients
            .Select(i => new { i.Id, RecipeId = recipe.Id, i.IngredientId, i.UnconfirmedIngredientId, i.Amount, i.UnitId, i.Note, i.SortOrder })
            .ToList();
        if (ingredients.Count > 0)
            await connection.ExecuteAsync(
                "SELECT insert_recipe_ingredient(@Id, @RecipeId, @IngredientId, @UnconfirmedIngredientId, @Amount, @UnitId, @Note, @SortOrder);",
                ingredients, transaction);
    }

    private static object ToParameters(Recipe r) => new
    {
        r.Id, r.OwnerUserId, r.Title, r.Description, r.CategoryId, r.CookTimeMinutes, r.Servings, r.ImageUrl,
        r.ImageAttribution, r.IsFavorite, SourceType = r.Source.Type.ToString(), SourceReference = r.Source.Reference,
        SourceUrl = r.Source.Url, SourceIsEditedFromSource = r.Source.IsEditedFromSource, r.CreatedAt, r.UpdatedAt
    };
}
