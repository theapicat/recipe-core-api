namespace RecipeCoreApi.Domain.Models.Recipes;

public class RecipeStep
{
    public required Guid Id { get; set; }
    public required Guid RecipeId { get; set; }
    public required int StepNumber { get; set; }
    public required string Description { get; set; }
    public int? TimerMinutes { get; set; }
}
