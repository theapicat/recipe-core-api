namespace Domain.Recipes;

public class RecipeListItem
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public string? ImageUrl { get; set; }
    public required Guid CategoryId { get; set; }
    public required int CookTimeMinutes { get; set; }
    public required int Servings { get; set; }
    public required bool IsFavorite { get; set; }
}
