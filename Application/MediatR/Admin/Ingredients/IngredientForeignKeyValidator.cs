using Application.MediatR.Catalog;
using Domain.Ingredients;
using MediatR;
using DomainUnit = Domain.Units.Unit;

namespace Application.MediatR.Admin.Ingredients;

// Sjekker at alle id-er en IngredientRequest peker på faktisk finnes, og at de henger sammen (standardenhet hører til
// den valgte enhetstypen, ingen dupliserte referanser, variantkjeden er syklusfri) - før noe skrives. Bruker de cachede
// katalog-listene (billig, hele lista er uansett i minnet) i stedet for ett oppslag per id.
public static class IngredientForeignKeyValidator
{
    // existingId = null ved opprettelse; ingrediensens egen id ved oppdatering (for å avvise selvreferanse/løkke i variantkjeden).
    public static async Task<string?> ValidateAsync(
        IMediator mediator, IngredientRequest request, Guid? existingId, CancellationToken cancellationToken)
    {
        var categories = await mediator.Send(new GetAllCatalogQuery<IngredientCategory>(), cancellationToken);
        if (categories.All(c => c.Id != request.CategoryId))
            return "Kategorien finnes ikke.";

        var unitTypes = await mediator.Send(new GetAllCatalogQuery<Domain.Units.UnitType>(), cancellationToken);
        if (unitTypes.All(t => t.Id != request.PrimaryUnitTypeId))
            return "Enhetstypen finnes ikke.";

        var units = await mediator.Send(new GetAllCatalogQuery<DomainUnit>(), cancellationToken);
        var unitById = units.ToDictionary(u => u.Id);
        if (!unitById.TryGetValue(request.DefaultUnitId, out var defaultUnit))
            return "Standardenheten finnes ikke.";
        if (defaultUnit.UnitTypeId != request.PrimaryUnitTypeId)
            return "Standardenheten må høre til den valgte enhetstypen.";

        if (request.AllergenIds.Count > 0)
        {
            var allergenIds = (await mediator.Send(new GetAllCatalogQuery<Allergen>(), cancellationToken))
                .Select(a => a.Id).ToHashSet();
            if (request.AllergenIds.Any(id => !allergenIds.Contains(id)))
                return "Ett eller flere allergener finnes ikke.";
        }

        if (request.SearchKeywordIds.Count > 0)
        {
            var keywordIds = (await mediator.Send(new GetAllCatalogQuery<SearchKeyword>(), cancellationToken))
                .Select(k => k.Id).ToHashSet();
            if (request.SearchKeywordIds.Any(id => !keywordIds.Contains(id)))
                return "Ett eller flere søkeord finnes ikke.";
        }

        if (request.NutrientValues.Count > 0)
        {
            var nutrientIds = (await mediator.Send(new GetAllCatalogQuery<NutrientDefinition>(), cancellationToken))
                .Select(n => n.Id).ToHashSet();
            var unknown = request.NutrientValues.Select(v => v.NutrientDefinitionId).FirstOrDefault(id => !nutrientIds.Contains(id));
            if (unknown is not null)
                return $"Næringsstoffet {unknown} finnes ikke.";
        }

        if (request.Portions.Count > 0)
        {
            foreach (var portion in request.Portions)
            {
                if (!unitById.ContainsKey(portion.UnitId))
                    return "Enheten i en porsjon finnes ikke.";
            }

            var duplicateUnitId = request.Portions.GroupBy(p => p.UnitId).FirstOrDefault(g => g.Count() > 1)?.Key;
            if (duplicateUnitId is { } dup)
                return $"Enheten {unitById[dup].Name} er brukt i flere porsjoner.";
        }

        if (request.VariantOfIngredientId is { } variantId)
        {
            var error = await ValidateVariantChainAsync(mediator, variantId, existingId, cancellationToken);
            if (error is not null)
                return error;
        }

        return null;
    }

    private static async Task<string?> ValidateVariantChainAsync(
        IMediator mediator, Guid variantId, Guid? existingId, CancellationToken cancellationToken)
    {
        if (existingId == variantId)
            return "En ingrediens kan ikke være en variant av seg selv.";

        var seen = new HashSet<Guid>();
        var currentId = variantId;
        while (true)
        {
            var current = await mediator.Send(new GetCatalogByIdQuery<Ingredient, Guid>(currentId), cancellationToken);
            if (current is null)
                return "Basisingrediensen finnes ikke.";
            if (current.Id == existingId || !seen.Add(current.Id))
                return "Variantkjeden danner en løkke.";
            if (current.VariantOfIngredientId is not { } next)
                return null;
            currentId = next;
        }
    }
}
