using Domain.Recipes;

namespace Persistence.Implementation;

// Flat rad fra get_recipe_by_id (Dapper matcher kolonnene på navn). Kilden (RecipeSource) ligger som kolonner i tabellen og bygges
// om til et nøstet objekt her, sammen med stegene og ingredienslinjene som leses separat.
public class RecipeRow
{
    public required Guid Id { get; set; }
    public required Guid OwnerUserId { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required Guid CategoryId { get; set; }
    public required int CookTimeMinutes { get; set; }
    public required int Servings { get; set; }
    public string? ImageUrl { get; set; }
    public string? ImageAttribution { get; set; }
    public required bool IsFavorite { get; set; }
    public required RecipeSourceType SourceType { get; set; }
    public string? SourceReference { get; set; }
    public string? SourceUrl { get; set; }
    public bool? SourceIsEditedFromSource { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    public required DateTimeOffset UpdatedAt { get; set; }

    public Recipe ToRecipe(List<RecipeStep> steps, List<RecipeIngredient> ingredients) => new()
    {
        Id = Id,
        OwnerUserId = OwnerUserId,
        Title = Title,
        Description = Description,
        CategoryId = CategoryId,
        CookTimeMinutes = CookTimeMinutes,
        Servings = Servings,
        ImageUrl = ImageUrl,
        ImageAttribution = ImageAttribution,
        IsFavorite = IsFavorite,
        Ingredients = ingredients,
        Steps = steps,
        Source = new RecipeSource
        {
            Type = SourceType,
            Reference = SourceReference,
            Url = SourceUrl,
            IsEditedFromSource = SourceIsEditedFromSource
        },
        CreatedAt = CreatedAt,
        UpdatedAt = UpdatedAt
    };
}
