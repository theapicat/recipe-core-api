using Domain.Ingredients;

namespace Application.MediatR.Admin.Ingredients;

public static class IngredientMapper
{
    // Returnerer en feilmelding, eller null hvis forespørselen er gyldig. Kun det som ikke fanges av databasen
    // på en brukbar måte (tomt navn, negative verdier); fremmednøkler og unikhet overlates til databasen.
    public static string? Validate(IngredientRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return "Navn må oppgis.";
        if (request.EnergyKcal < 0)
            return "Energi (kcal) kan ikke være negativ.";
        if (request.NutrientValues.Any(v => string.IsNullOrWhiteSpace(v.NutrientDefinitionId) || v.Quantity < 0))
            return "Næringsverdier må ha et næringsstoff og en verdi som ikke er negativ.";
        if (request.Portions.Any(p => p.GramsPerPortion <= 0))
            return "Porsjoner må ha en gramverdi større enn null.";
        return null;
    }

    // Tildeler id til ingrediensen og alle barna (UUIDv7). Allergen-/nøkkelord-id-er dedupliseres siden
    // koblingstabellene har sammensatt primærnøkkel.
    public static Ingredient ToIngredient(IngredientRequest request, Guid id) => new()
    {
        Id = id,
        Name = request.Name.Trim(),
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
}
