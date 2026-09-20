namespace Domain.Ingredients;

// Statisk gruppering av næringsstoffene (kun seed-data): fett, karbohydrat, protein, vitaminer, mineraler, sporstoffer, annet -
// med undergrupper (mettede/enumettede/flerumettede fettsyrer under fett, vitamin a-e under vitaminer). Gir brukeren valget
// mellom en overordnet og en dypere visning.
//
// Ligger bevisst nøstet i NutrientDefinition.Group og har ingen egne endepunkter, Reader eller Writer: gruppene leses bare som
// en del av et næringsstoff (klienten bygger visningen ut fra dem). Id er en Guid som alle andre kataloger.
public class NutrientGroup
{
    public required Guid Id { get; set; }

    // Små bokstaver, som øvrige kategorinavn - frontend gjør om til stor forbokstav ved visning.
    public required string Name { get; set; }

    // Visningsrekkefølge for gruppene (dybde-først).
    public required int SortOrder { get; set; }

    // Overordnet gruppe for en undergruppe (f.eks. "fett" for "mettede fettsyrer"). Null for hovedgrupper. Overordnet gruppe
    // har selv aldri en ParentGroup (kun to nivåer).
    public NutrientGroup? ParentGroup { get; set; }
}
