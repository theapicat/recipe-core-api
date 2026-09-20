using Domain.Recipes;

namespace Application.MediatR.User.Recipes;

// Innhold brukeren sender inn for å opprette/erstatte en oppskrift. Bevisst uten id-er, steg-nummer og avledede felt: serveren
// tildeler id-er (oppskriften og alle barna), nummererer stegene og ingrediensene etter rekkefølge, regner ut koketiden (summen av
// steg-timerne), og bestemmer kilde-type (Manual for oppskrifter brukeren lager) og tidsstempler. Eier kommer fra tokenet.
public record RecipeRequest
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required Guid CategoryId { get; init; }
    public required int Servings { get; init; }
    public string? ImageUrl { get; init; }
    public string? ImageAttribution { get; init; }
    public RecipeSourceRequest? Source { get; init; }
    public List<RecipeStepRequest> Steps { get; init; } = [];
    public List<RecipeIngredientRequest> Ingredients { get; init; } = [];
}

// Kun fritekstreferansen (f.eks. kokebok-tittel) kan oppgis - type og url settes av serveren.
public record RecipeSourceRequest
{
    public string? Reference { get; init; }
}

public record RecipeStepRequest
{
    public required string Description { get; init; }
    public int? TimerMinutes { get; init; }
}

// Nøyaktig én av IngredientId og UnconfirmedIngredientId. Amount er valgfri: utelatt/0 betyr «etter smak».
public record RecipeIngredientRequest
{
    public Guid? IngredientId { get; init; }
    public Guid? UnconfirmedIngredientId { get; init; }
    public decimal Amount { get; init; }
    public required Guid UnitId { get; init; }
    public string? Note { get; init; }
}
