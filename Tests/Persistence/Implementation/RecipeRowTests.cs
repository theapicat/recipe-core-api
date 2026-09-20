using Domain.Recipes;
using Persistence.Implementation;
using Xunit;

namespace Tests.Persistence.Implementation;

public class RecipeRowTests
{
    [Fact]
    public void ToRecipe_BuildsTheNestedSource_AndKeepsTheStepsAndIngredients()
    {
        var row = new RecipeRow
        {
            Id = Guid.NewGuid(), OwnerUserId = Guid.NewGuid(), Title = "pannekaker", Description = "Enkle pannekaker.",
            CategoryId = Guid.NewGuid(), CookTimeMinutes = 10, Servings = 4, IsFavorite = true,
            SourceType = RecipeSourceType.Scraped, SourceReference = "kokebok", SourceUrl = "https://example.test/x",
            SourceIsEditedFromSource = true, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow
        };
        var step = new RecipeStep { Id = Guid.NewGuid(), RecipeId = row.Id, StepNumber = 1, Description = "Stek." };
        var line = new RecipeIngredient
        {
            Id = Guid.NewGuid(), RecipeId = row.Id, IngredientId = Guid.NewGuid(), Amount = 3, UnitId = Guid.NewGuid(), SortOrder = 1, Name = "egg"
        };

        var recipe = row.ToRecipe([step], [line]);

        Assert.Equal(row.Id, recipe.Id);
        Assert.Equal(RecipeSourceType.Scraped, recipe.Source.Type);
        Assert.Equal("kokebok", recipe.Source.Reference);
        Assert.Equal("https://example.test/x", recipe.Source.Url);
        Assert.True(recipe.Source.IsEditedFromSource);
        Assert.Same(step, Assert.Single(recipe.Steps));
        Assert.Same(line, Assert.Single(recipe.Ingredients));
    }
}
