using Domain.Recipes;
using Domain.Units;

namespace Application.MediatR.User.Recipes;

// Ren beregning (ingen database, ingen tilstand) av næringen i en oppskrift - enhetstestbar. Regner ut fra dagens ingrediensdata hver gang,
// så resultatet aldri er utdatert. Veiledende, ikke absolutt.
public static class RecipeNutritionCalculator
{
    private const int Decimals = 4;

    public static RecipeNutrition Calculate(RecipeNutritionInput input)
    {
        var servings = Math.Max(1, input.Servings);
        var portions = input.Portions.ToLookup(p => p.IngredientId);
        var values = input.Values.ToLookup(v => v.IngredientId);

        decimal kcal = 0, kj = 0;
        var nutrients = new Dictionary<string, (decimal Total, int Order)>();
        var skipped = new List<RecipeNutritionSkippedLine>();
        var counted = 0;

        foreach (var line in input.Lines.OrderBy(l => l.SortOrder))
        {
            if (line.Amount <= 0)
            {
                skipped.Add(Skip(line, NutritionSkipReason.ToTaste));
                continue;
            }

            if (line.IngredientId is not { } ingredientId)
            {
                skipped.Add(Skip(line, NutritionSkipReason.Unconfirmed));
                continue;
            }

            var edibleGrams = ToEdibleGrams(line, portions[ingredientId].ToList());
            if (edibleGrams is null)
            {
                skipped.Add(Skip(line, NutritionSkipReason.NoConversion));
                continue;
            }

            counted++;

            // Alle verdier er per 100 g spiselig del.
            var factor = edibleGrams.Value / 100m;
            kcal += (line.EnergyKcal ?? 0) * factor;
            kj += (line.EnergyKj ?? 0) * factor;

            foreach (var value in values[ingredientId])
            {
                var previous = nutrients.GetValueOrDefault(value.NutrientDefinitionId);
                nutrients[value.NutrientDefinitionId] = (previous.Total + value.Quantity * factor, value.NutrientSortOrder);
            }
        }

        return new RecipeNutrition
        {
            RecipeId = input.RecipeId,
            Servings = servings,
            EnergyKcal = ToAmount(kcal, servings),
            EnergyKj = ToAmount(kj, servings),
            Nutrients = nutrients
                .Select(n => (n.Key, n.Value.Order, Total: Math.Round(n.Value.Total, Decimals)))
                .Where(n => n.Total > 0)
                .OrderBy(n => n.Order)
                .ThenBy(n => n.Key, StringComparer.Ordinal)
                .Select(n => new RecipeNutrientTotal
                {
                    NutrientId = n.Key,
                    Total = n.Total,
                    PerServing = Math.Round(n.Total / servings, Decimals)
                })
                .ToList(),
            CountedIngredients = counted,
            TotalIngredients = input.Lines.Count,
            SkippedLines = skipped
        };
    }

    // Gram SPISELIG del for linjen, eller null hvis enheten ikke kan omregnes. Rekkefølge:
    // 1. Ingrediensens egen porsjon for akkurat denne enheten (f.eks. 1 stk = 120 g). Porsjonsvektene i Matvaretabellen er vekt av
    //    spiselig del (banan: 1 stk = 120 g, 66 % spiselig - stemmer med en skrellet banan), så det gjøres INGEN fratrekk for uspiselig del.
    // 2. Vektenhet: mengden er vekt slik den er innkjøpt (500 g hel banan) -> trekk fra uspiselig del (EdiblePartPercent).
    // 3. Volumenhet uten egen porsjon: skaler via ingrediensens største volumporsjon (gram per ml), f.eks. 1 ss ut fra dl-vekten. Volum
    //    måles på spiselig/tilberedt mat, så heller ikke her noe fratrekk.
    private static decimal? ToEdibleGrams(RecipeNutritionLine line, List<RecipeNutritionPortion> ingredientPortions)
    {
        var exact = ingredientPortions.FirstOrDefault(p => p.UnitId == line.UnitId);
        if (exact is not null)
            return line.Amount * exact.GramsPerPortion;

        if (line.UnitTypeName == UnitTypeNames.Weight)
            return line.Amount * line.UnitBaseRatio * EdibleFraction(line.EdiblePartPercent);

        if (line.UnitTypeName == UnitTypeNames.Volume)
        {
            var volumePortion = ingredientPortions
                .Where(p => p.UnitTypeName == UnitTypeNames.Volume && p.UnitBaseRatio > 0)
                .OrderByDescending(p => p.UnitBaseRatio)
                .FirstOrDefault();
            if (volumePortion is not null)
                return line.Amount * line.UnitBaseRatio * (volumePortion.GramsPerPortion / volumePortion.UnitBaseRatio);
        }

        return null;
    }

    // Ukjent (null), 0 eller 100 = alt er spiselig.
    private static decimal EdibleFraction(decimal? percent) => percent is > 0 and < 100 ? percent.Value / 100m : 1m;

    private static NutritionAmount? ToAmount(decimal total, int servings)
    {
        var rounded = Math.Round(total, Decimals);
        return rounded > 0 ? new NutritionAmount { Total = rounded, PerServing = Math.Round(rounded / servings, Decimals) } : null;
    }

    private static RecipeNutritionSkippedLine Skip(RecipeNutritionLine line, NutritionSkipReason reason) => new()
    {
        RecipeIngredientId = line.LineId,
        Name = line.Name,
        Reason = reason
    };
}
