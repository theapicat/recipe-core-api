namespace Application.MediatR.Admin.Ingredients;

// Innhold admin sender inn for å opprette/oppdatere en ingrediens. Bevisst uten id-er: serveren tildeler dem
// (ingrediensen og alle barna), så klienten trenger ikke generere ~60 id-er per ingrediens.
public record IngredientRequest
{
    public required string Name { get; init; }
    public required Guid CategoryId { get; init; }
    public required Guid PrimaryUnitTypeId { get; init; }
    public required Guid DefaultUnitId { get; init; }
    public required decimal EnergyKcal { get; init; }
    public decimal? EnergyKj { get; init; }
    public decimal? EdiblePartPercent { get; init; }
    public string? SourceId { get; init; }
    public string? SourceUrl { get; init; }

    // Satt når dette er en variant av en annen ingrediens. Næringsdata fylles ut av klienten (typisk forhåndsutfylt
    // fra basisingrediensen) - serveren kopierer ikke selv.
    public Guid? VariantOfIngredientId { get; init; }

    public bool IsVerified { get; init; }
    public List<Guid> AllergenIds { get; init; } = [];
    public List<Guid> SearchKeywordIds { get; init; } = [];
    public List<IngredientNutrientValueRequest> NutrientValues { get; init; } = [];
    public List<IngredientPortionRequest> Portions { get; init; } = [];
}

public record IngredientNutrientValueRequest
{
    public required string NutrientDefinitionId { get; init; }
    public required decimal Quantity { get; init; }
    public string? SourceId { get; init; }
}

public record IngredientPortionRequest
{
    public required Guid UnitId { get; init; }
    public required decimal GramsPerPortion { get; init; }
}
