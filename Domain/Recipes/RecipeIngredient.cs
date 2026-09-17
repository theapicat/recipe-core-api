namespace RecipeCoreApi.Domain.Models.Recipes;

// Peker på nøyaktig én av IngredientId eller UnconfirmedIngredientId, aldri begge/ingen.
public class RecipeIngredient
{
    public required Guid Id { get; set; }
    public required Guid RecipeId { get; set; }
    public Guid? IngredientId { get; set; }
    public Guid? UnconfirmedIngredientId { get; set; }
    public required decimal Amount { get; set; }
    public required Guid UnitId { get; set; }
    public string? Note { get; set; }
}
