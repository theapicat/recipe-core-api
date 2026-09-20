namespace Domain.Ingredients;

// Statisk, skrivebeskyttet katalog (kun seed-data, ingen skrive-endepunkter). Speiler Matvaretabellens næringsstoff-katalog.
// Id er kildens egen kode (f.eks. "Fett", "Vit C"), ikke en generert Guid - koden er allerede stabil og unik.
public class NutrientDefinition : IHasId<string>
{
    public required string Id { get; set; }
    public required string Name { get; set; }

    // Måleenheten er en fremmednøkkel til unit. Lesespørringen folder ut forkortelsen (Unit: "mg", "µg" ...) og enhetstypen
    // (UnitTypeId: vekt), slik at klienten kan vise og omregne/summere (via enhetens BaseUnitRatio) uten et ekstra oppslag.
    public required Guid UnitId { get; set; }
    public required string Unit { get; set; }
    public required Guid UnitTypeId { get; set; }

    public required int DecimalPrecision { get; set; }

    // Gruppen (den innerste, f.eks. "vitamin c" eller "mettede fettsyrer") stoffet hører til, med overordnet gruppe nøstet
    // inni (Group.ParentGroup) - se NutrientGroup. Hierarkiet ligger i gruppene, ikke i en parent på stoffet. Første stoff i
    // en undergruppe er summen for gruppen, resten er delverdiene.
    public required NutrientGroup Group { get; set; }

    public string? SourceUrl { get; set; }

    // Visningsrekkefølge i hele lista (gruppe for gruppe, dybde-først), satt av seed-dataene.
    public required int SortOrder { get; set; }
}
