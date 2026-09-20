using Application.MediatR.User.Recipes;
using Domain.Recipes;

namespace Tests.Application.MediatR.User.Recipes;

internal static class RecipeTestData
{
    public static RecipeRequest ValidRequest() => new()
    {
        Title = "Kremet Kyllinggryte",
        Description = "Enkel hverdagsmiddag.",
        CategoryId = Guid.NewGuid(),
        Servings = 4,
        Steps =
        [
            new RecipeStepRequest { Description = "Kutt kyllingen." },
            new RecipeStepRequest { Description = "Kok i 20 minutter.", TimerMinutes = 20 }
        ],
        Ingredients =
        [
            new RecipeIngredientRequest { IngredientId = Guid.NewGuid(), Amount = 500, UnitId = Guid.NewGuid() }
        ]
    };

    public static Recipe ExistingRecipe(Guid ownerUserId, RecipeSourceType type = RecipeSourceType.Manual) => new()
    {
        Id = Guid.NewGuid(),
        OwnerUserId = ownerUserId,
        Title = "gammel tittel",
        Description = "Gammel beskrivelse.",
        CategoryId = Guid.NewGuid(),
        CookTimeMinutes = 0,
        Servings = 2,
        IsFavorite = true,
        Ingredients = [],
        Steps = [],
        Source = new RecipeSource
        {
            Type = type,
            Reference = "gammel kilde",
            Url = type == RecipeSourceType.Scraped ? "https://example.test/oppskrift" : null,
            IsEditedFromSource = type == RecipeSourceType.Scraped ? false : null
        },
        CreatedAt = DateTimeOffset.UtcNow.AddDays(-3),
        UpdatedAt = DateTimeOffset.UtcNow.AddDays(-3)
    };
}
