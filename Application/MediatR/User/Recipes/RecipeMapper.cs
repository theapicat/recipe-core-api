using Application.Naming;
using Domain.Recipes;

namespace Application.MediatR.User.Recipes;

public static class RecipeMapper
{
    // Returnerer en feilmelding, eller null hvis forespørselen er gyldig. Kun det som ikke fanges av databasen på en brukbar måte
    // (tomme felt, minst ett steg og én ingrediens, formater og grenser); fremmednøkler (kategori, ingrediens, enhet) overlates til
    // databasen (409).
    public static string? Validate(RecipeRequest request)
    {
        var title = NameNormalizer.Normalize(request.Title);
        if (title.Length == 0)
            return "Tittel må oppgis.";
        if (title.Length > RecipeLimits.MaxTitleLength)
            return $"Tittelen kan ha maks {RecipeLimits.MaxTitleLength} tegn.";

        var description = request.Description?.Trim() ?? string.Empty;
        if (description.Length == 0)
            return "Beskrivelse må oppgis.";
        if (description.Length > RecipeLimits.MaxDescriptionLength)
            return $"Beskrivelsen kan ha maks {RecipeLimits.MaxDescriptionLength} tegn.";

        if (request.Servings < 1 || request.Servings > RecipeLimits.MaxServings)
            return $"Antall porsjoner må være mellom 1 og {RecipeLimits.MaxServings}.";

        var imageError = ValidateImage(request);
        if (imageError is not null)
            return imageError;

        if (request.Source?.Reference is { } reference && reference.Trim().Length > RecipeLimits.MaxReferenceLength)
            return $"Kildereferansen kan ha maks {RecipeLimits.MaxReferenceLength} tegn.";

        return ValidateSteps(request.Steps) ?? ValidateIngredients(request.Ingredients);
    }

    // Bygger oppskriften med server-tildelte id-er. Stegene og ingrediensene nummereres 1..n etter rekkefølge, og koketiden er summen av
    // steg-timerne (ikke et innskrevet felt). Kilde, favoritt og tidsstempler avgjøres av kalleren (opprett vs. oppdater).
    public static Recipe ToRecipe(RecipeRequest request, Guid id, Guid ownerUserId, RecipeSource source, bool isFavorite,
        DateTimeOffset createdAt, DateTimeOffset updatedAt)
    {
        var steps = request.Steps
            .Select((s, index) => new RecipeStep
            {
                Id = Guid.CreateVersion7(),
                RecipeId = id,
                StepNumber = index + 1,
                Description = s.Description.Trim(),
                TimerMinutes = s.TimerMinutes
            })
            .ToList();

        var ingredients = request.Ingredients
            .Select((i, index) => new RecipeIngredient
            {
                Id = Guid.CreateVersion7(),
                RecipeId = id,
                IngredientId = i.IngredientId,
                UnconfirmedIngredientId = i.UnconfirmedIngredientId,
                Amount = i.Amount,
                UnitId = i.UnitId,
                Note = NullIfBlank(i.Note),
                SortOrder = index + 1
            })
            .ToList();

        return new Recipe
        {
            Id = id,
            OwnerUserId = ownerUserId,
            Title = NameNormalizer.Normalize(request.Title),
            Description = request.Description.Trim(),
            CategoryId = request.CategoryId,
            CookTimeMinutes = steps.Sum(s => s.TimerMinutes ?? 0),
            Servings = request.Servings,
            ImageUrl = NullIfBlank(request.ImageUrl),
            ImageAttribution = NullIfBlank(request.ImageAttribution),
            IsFavorite = isFavorite,
            Ingredients = ingredients,
            Steps = steps,
            Source = source,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }

    public static string? NullIfBlank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? ValidateImage(RecipeRequest request)
    {
        var url = NullIfBlank(request.ImageUrl);
        if (url is not null)
        {
            if (url.Length > RecipeLimits.MaxImageUrlLength)
                return "Bilde-adressen er for lang.";
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                return "Bilde-adressen må være en gyldig http- eller https-adresse.";
        }

        if (NullIfBlank(request.ImageAttribution) is { } attribution && attribution.Length > RecipeLimits.MaxImageAttributionLength)
            return $"Bildekreditering kan ha maks {RecipeLimits.MaxImageAttributionLength} tegn.";

        return null;
    }

    private static string? ValidateSteps(List<RecipeStepRequest> steps)
    {
        if (steps.Count == 0)
            return "Oppskriften må ha minst ett steg.";
        if (steps.Count > RecipeLimits.MaxSteps)
            return $"Oppskriften kan ha maks {RecipeLimits.MaxSteps} steg.";

        foreach (var step in steps)
        {
            var text = step.Description?.Trim() ?? string.Empty;
            if (text.Length == 0)
                return "Alle steg må ha en beskrivelse.";
            if (text.Length > RecipeLimits.MaxStepLength)
                return $"Et steg kan ha maks {RecipeLimits.MaxStepLength} tegn.";
            if (step.TimerMinutes is < 0 or > RecipeLimits.MaxTimerMinutes)
                return $"Timeren i et steg må være mellom 0 og {RecipeLimits.MaxTimerMinutes} minutter.";
        }

        return null;
    }

    private static string? ValidateIngredients(List<RecipeIngredientRequest> ingredients)
    {
        if (ingredients.Count == 0)
            return "Oppskriften må ha minst én ingrediens.";
        if (ingredients.Count > RecipeLimits.MaxIngredients)
            return $"Oppskriften kan ha maks {RecipeLimits.MaxIngredients} ingredienser.";

        foreach (var line in ingredients)
        {
            if (line.IngredientId.HasValue == line.UnconfirmedIngredientId.HasValue)
                return "Hver ingrediens må peke på nøyaktig én ingrediens (offisiell eller din egen).";
            if (line.Amount < 0 || line.Amount > RecipeLimits.MaxAmount)
                return "Mengden kan ikke være negativ. Bruk 0 for «etter smak».";
            if (NullIfBlank(line.Note) is { } note && note.Length > RecipeLimits.MaxNoteLength)
                return $"Et notat kan ha maks {RecipeLimits.MaxNoteLength} tegn.";
        }

        return null;
    }
}
