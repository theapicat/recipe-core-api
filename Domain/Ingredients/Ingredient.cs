namespace Domain.Ingredients;

public class Ingredient : IHasId<Guid>
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required Guid CategoryId { get; set; }
    public required List<Guid> AllergenIds { get; set; }
    public required Guid PrimaryUnitTypeId { get; set; }
    public required Guid DefaultUnitId { get; set; }
    public required decimal EnergyKcal { get; set; }
    public decimal? EnergyKj { get; set; }

    // Andel av matvaren som er spiselig (f.eks. 97 for agurk).
    public decimal? EdiblePartPercent { get; set; }

    public required List<Guid> SearchKeywordIds { get; set; }

    // Kun satt for offisielt importerte ingredienser - lar brukeren slå opp kilden.
    public string? SourceId { get; set; }
    public string? SourceUrl { get; set; }

    // Satt når ingrediensen er en variant av en annen (f.eks. en spesiell gulrotsort). Næringsdata er
    // kopiert fra basisingrediensen ved opprettelse og følger den ikke videre - næringsverdier er veiledende.
    public Guid? VariantOfIngredientId { get; set; }

    public required List<IngredientNutrientValue> NutrientValues { get; set; }
    public required List<IngredientPortion> Portions { get; set; }

    // false for adminlagte innslag som venter på fullstendige nærings-/allergendata.
    public required bool IsVerified { get; set; }
}
