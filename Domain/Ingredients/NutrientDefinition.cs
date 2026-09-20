namespace Domain.Ingredients;

// Statisk, skrivebeskyttet katalog (kun seed-data, ingen skrive-endepunkter). Speiler Matvaretabellens næringsstoff-katalog. Id er kildens egen kode (f.eks. "Fett", "Vit C"),
// ikke en generert Guid - katalogen importeres derfra, koden er allerede stabil og unik. Derfor
// oppgis Id av admin (tildeles ikke av serveren).
public class NutrientDefinition : IHasId<string>
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Unit { get; set; }
    public required int DecimalPrecision { get; set; }

    // Selvreferanse for hierarki (f.eks. "Mettet" -> ParentId "Fett"). Ingen ParentId = toppnivå/basic-visning.
    public string? ParentId { get; set; }

    public string? SourceUrl { get; set; }

    // true for rene grupperader (mineraler, sporstoffer, vitamingrupper): en overskrift uten egen verdi. Rader med
    // barn som *har* egen verdi (Fett, Karbohydrat ...) er ikke grupper - de er totalen, barna er delverdiene.
    public required bool IsGroup { get; set; }

    // Visningsrekkefølge: dybde-først gjennom hierarkiet, satt av seed-dataene.
    public required int SortOrder { get; set; }
}
