namespace RecipeCoreApi.Domain.Models.Recipes;

public class RecipeSource
{
    public required RecipeSourceType Type { get; set; }

    // Fritekstreferanse, f.eks. kokebok-tittel. Redigerbar uansett type.
    public string? Reference { get; set; }

    // Kun satt for Type == Scraped - låst, kan ikke fjernes av brukeren.
    public string? Url { get; set; }

    // Satt til true når brukeren har redigert en scrapet oppskrift etter import.
    public bool? IsEditedFromSource { get; set; }
}
