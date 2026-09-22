using Application.Naming;
using Domain.Ingredients;

namespace Application.MediatR.Admin.Ingredients;

public static class IngredientMapper
{
    // Returnerer en feilmelding, eller null hvis forespørselen er gyldig. Kun det som ikke krever et databaseoppslag
    // (tomme/ugyldige felt, negative verdier, tall som er for store til å lagres, dupliserte referanser); eksistensen av
    // fremmednøkler (kategori, enheter, allergener, søkeord, næringsstoffer, variant) sjekkes av IngredientForeignKeyValidator.
    public static string? Validate(IngredientRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return "Navn må oppgis.";
        if (request.EnergyKcal < 0)
            return "Energi (kcal) kan ikke være negativ.";
        if (request.EnergyKj is < 0)
            return "Energi (kJ) kan ikke være negativ.";
        if (request.EdiblePartPercent is { } edible && (edible <= 0 || edible > 100))
            return "Spiselig del må være mellom 0 og 100 %.";
        if (!FitsNumeric(request.EnergyKcal, 10, 2) || request.EnergyKj is { } kj && !FitsNumeric(kj, 10, 2))
            return "Verdien for energi er for stor.";
        if (request.SourceUrl is { } sourceUrl && !IsHttpUrl(sourceUrl))
            return "Kilde-URL må starte med http:// eller https://.";

        if (request.NutrientValues.Any(v => string.IsNullOrWhiteSpace(v.NutrientDefinitionId) || v.Quantity < 0))
            return "Næringsverdier må ha et næringsstoff og en verdi som ikke er negativ.";
        if (request.NutrientValues.Any(v => !FitsNumeric(v.Quantity, 12, 4)))
            return "Verdien for et næringsstoff er for stor.";
        var duplicateNutrient = request.NutrientValues
            .GroupBy(v => v.NutrientDefinitionId)
            .FirstOrDefault(g => g.Count() > 1);
        if (duplicateNutrient is not null)
            return $"Næringsstoffet {duplicateNutrient.Key} er oppgitt flere ganger.";
        if (request.IsVerified && request.NutrientValues.Count == 0)
            return "Ingrediensen må ha næringsverdier for å kunne verifiseres.";

        if (request.Portions.Any(p => p.GramsPerPortion <= 0))
            return "Porsjoner må ha en gramverdi større enn null.";
        if (request.Portions.Any(p => !FitsNumeric(p.GramsPerPortion, 10, 2)))
            return "Verdien for en porsjon er for stor.";

        return null;
    }

    // Kun kalt når den lagrede ingrediensen har IsOfficial = true. Kildedata (navn, energi, spiselig del, kilde-id/-url og
    // selve settet av næringsverdier) kan ikke endres på en offisiell ingrediens - oppfordre til å opprette en variant i
    // stedet. Kategori, enheter, allergener, søkeord, porsjoner og IsVerified er fortsatt fritt redigerbare.
    public static string? ValidateOfficialLock(Ingredient existing, IngredientRequest request)
    {
        var nameChanged = NameNormalizer.Normalize(request.Name) != existing.Name;
        var energyChanged = request.EnergyKcal != existing.EnergyKcal || request.EnergyKj != existing.EnergyKj;
        var edibleChanged = request.EdiblePartPercent != existing.EdiblePartPercent;
        var sourceChanged = request.SourceId != existing.SourceId || request.SourceUrl != existing.SourceUrl;
        var nutrientsChanged = !NutrientValueSetsAreEqual(existing.NutrientValues, request.NutrientValues);

        return nameChanged || energyChanged || edibleChanged || sourceChanged || nutrientsChanged
            ? "Offisielle ingredienser kan ikke endre kildedata. Opprett en variant."
            : null;
    }

    // Tildeler id til ingrediensen og alle barna (UUIDv7). Allergen-/nøkkelord-id-er dedupliseres siden
    // koblingstabellene har sammensatt primærnøkkel. isOfficial, updatedAt og createdAt avgjøres av kalleren
    // (aldri av request-body) - createdAt er now() ved opprettelse, videreført fra existing.CreatedAt ved oppdatering.
    // UsageCount settes alltid til 0: beregnes kun ved lesing (get_ingredient_by_id), aldri lagret eller skrevet.
    public static Ingredient ToIngredient(
        IngredientRequest request, Guid id, bool isOfficial, DateTimeOffset updatedAt, DateTimeOffset createdAt) => new()
    {
        Id = id,
        Name = NameNormalizer.Normalize(request.Name),
        CategoryId = request.CategoryId,
        PrimaryUnitTypeId = request.PrimaryUnitTypeId,
        DefaultUnitId = request.DefaultUnitId,
        EnergyKcal = request.EnergyKcal,
        EnergyKj = request.EnergyKj,
        EdiblePartPercent = request.EdiblePartPercent,
        SourceId = request.SourceId,
        SourceUrl = request.SourceUrl,
        VariantOfIngredientId = request.VariantOfIngredientId,
        IsVerified = request.IsVerified,
        IsOfficial = isOfficial,
        UpdatedAt = updatedAt,
        CreatedAt = createdAt,
        AllergensReviewed = request.AllergensReviewed,
        UsageCount = 0,
        AllergenIds = request.AllergenIds.Distinct().ToList(),
        SearchKeywordIds = request.SearchKeywordIds.Distinct().ToList(),
        NutrientValues = request.NutrientValues
            .Select(v => new IngredientNutrientValue
            {
                Id = Guid.CreateVersion7(),
                IngredientId = id,
                NutrientDefinitionId = v.NutrientDefinitionId,
                Quantity = v.Quantity,
                SourceId = v.SourceId
            })
            .ToList(),
        Portions = request.Portions
            .Select(p => new IngredientPortion
            {
                Id = Guid.CreateVersion7(),
                IngredientId = id,
                UnitId = p.UnitId,
                GramsPerPortion = p.GramsPerPortion
            })
            .ToList()
    };

    private static bool NutrientValueSetsAreEqual(List<IngredientNutrientValue> existing, List<IngredientNutrientValueRequest> requested)
    {
        if (existing.Count != requested.Count)
            return false;

        var existingSet = existing.Select(v => (v.NutrientDefinitionId, v.Quantity, v.SourceId)).ToHashSet();
        var requestedSet = requested.Select(v => (v.NutrientDefinitionId, v.Quantity, v.SourceId)).ToHashSet();
        return existingSet.SetEquals(requestedSet);
    }

    private static bool IsHttpUrl(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    // Sjekker at verdien får plass i en Postgres numeric(precision, scale)-kolonne, uten flyttallsavrunding.
    private static bool FitsNumeric(decimal value, int precision, int scale)
    {
        var max = (Pow10(precision) - 1) / Pow10(scale);
        return Math.Abs(value) <= max;
    }

    private static decimal Pow10(int exponent)
    {
        decimal result = 1;
        for (var i = 0; i < exponent; i++)
            result *= 10;
        return result;
    }
}
