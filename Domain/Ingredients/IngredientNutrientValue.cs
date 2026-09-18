namespace Domain.Ingredients;

// Kun én rad per (IngredientId, NutrientDefinitionId) som faktisk er målt - ikke alle
// ingredienser har verdi for alle næringsstoffer.
public class IngredientNutrientValue
{
    public required Guid Id { get; set; }
    public required Guid IngredientId { get; set; }
    public required string NutrientDefinitionId { get; set; }
    public required decimal Quantity { get; set; }

    // Kildens egen referansekode for denne spesifikke verdien (sporbarhet utover Ingredient.SourceId).
    public string? SourceId { get; set; }
}
