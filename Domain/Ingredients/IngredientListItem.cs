namespace Domain.Ingredients;

// Lettvekts-projeksjon av Ingredient for søk/valg (f.eks. når en bruker lager en oppskrift). Hele lista
// caches og filtreres i minnet. Inneholder allergen-/nøkkelord-id-er slik at filtrering ikke trenger
// den tunge Ingredient-modellen. Arrays (ikke List) fordi Npgsql leverer uuid[] som Guid[].
public class IngredientListItem
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required Guid CategoryId { get; set; }
    public required Guid PrimaryUnitTypeId { get; set; }
    public required Guid DefaultUnitId { get; set; }
    public required decimal EnergyKcal { get; set; }
    public required bool IsVerified { get; set; }

    // true kun for rader fra den offisielle kilden (Matvaretabellen-seeden) - se Ingredient.IsOfficial.
    public required bool IsOfficial { get; set; }

    public Guid? VariantOfIngredientId { get; set; }
    public required Guid[] AllergenIds { get; set; }
    public required Guid[] SearchKeywordIds { get; set; }
}
