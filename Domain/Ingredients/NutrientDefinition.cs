namespace RecipeCoreApi.Domain.Models.Ingredients;

// Speiler Matvaretabellens næringsstoff-katalog. Id er kildens egen kode (f.eks. "Fett", "Vit C"),
// ikke en generert Guid - katalogen importeres derfra, koden er allerede stabil og unik.
public class NutrientDefinition
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Unit { get; set; }
    public required int DecimalPrecision { get; set; }

    // Selvreferanse for hierarki (f.eks. "Mettet" -> ParentId "Fett"). Ingen ParentId = toppnivå/basic-visning.
    public string? ParentId { get; set; }

    public string? SourceUrl { get; set; }
}
