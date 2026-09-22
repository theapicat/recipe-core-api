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

    // true kun for rader fra den offisielle kilden (Matvaretabellen-seeden). Tildeles av serveren ved opprettelse og kan
    // aldri endres via PUT - en admin-opprettet eller brukergodkjent ingrediens er alltid false.
    public required bool IsOfficial { get; set; }

    // Brukes til optimistisk samtidighetskontroll: IngredientRequest.UpdatedAt sendes tilbake på PUT og sammenlignes mot
    // denne før skriving.
    public required DateTimeOffset UpdatedAt { get; set; }

    // Satt av serveren ved opprettelse, endres aldri (updated_at har ikke egen p_created_at-parameter i update_ingredient).
    public required DateTimeOffset CreatedAt { get; set; }

    // Satt av admin når allergen-tilordningen er verifisert komplett/korrekt - uavhengig av IsVerified/IsOfficial,
    // og fritt redigerbar selv om ingrediensen er offisiell (se IngredientMapper.ValidateOfficialLock).
    public required bool AllergensReviewed { get; set; }

    // Beregnet ved lesing (recipe_ingredient + varianter + løste ubekreftede ingredienser), aldri lagret eller skrevet -
    // ToIngredient setter alltid 0, faktisk verdi kommer kun fra get_ingredient_by_id.
    public int UsageCount { get; set; }
}
