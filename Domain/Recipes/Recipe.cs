namespace Domain.Recipes;

public class Recipe
{
    public required Guid Id { get; set; }
    public required Guid OwnerUserId { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required Guid CategoryId { get; set; }

    // Summen av TimerMinutes for stegene som har en timer - ikke et separat inntastet felt.
    public required int CookTimeMinutes { get; set; }

    public required int Servings { get; set; }
    public string? ImageUrl { get; set; }
    public string? ImageAttribution { get; set; }
    public required bool IsFavorite { get; set; }
    public required List<RecipeIngredient> Ingredients { get; set; }
    public required List<RecipeStep> Steps { get; set; }
    public required RecipeSource Source { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    public required DateTimeOffset UpdatedAt { get; set; }
}
