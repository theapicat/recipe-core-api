using Application.MediatR.User.Recipes;
using Domain.Recipes;
using Domain.Units;
using Xunit;

namespace Tests.Application.MediatR.User.Recipes;

public class RecipeNutritionCalculatorTests
{
    private static readonly Guid Gram = Guid.NewGuid();
    private static readonly Guid Kilogram = Guid.NewGuid();
    private static readonly Guid Deciliter = Guid.NewGuid();
    private static readonly Guid Tablespoon = Guid.NewGuid();
    private static readonly Guid Piece = Guid.NewGuid();
    private static readonly Guid Pinch = Guid.NewGuid();

    // Per 100 g: 100 kcal (418 kJ), Fett 10 g, Protein 5 g.
    private static RecipeNutritionLine Line(Guid? ingredientId, decimal amount, Guid unitId, string unitType, decimal ratio,
        int sortOrder = 1, decimal? edible = null, decimal? kcal = 100, decimal? kj = 418) => new()
    {
        LineId = Guid.NewGuid(), SortOrder = sortOrder, Name = ingredientId is null ? "egen" : "ingrediens", IngredientId = ingredientId,
        Amount = amount, UnitId = unitId, UnitTypeName = unitType, UnitBaseRatio = ratio, EnergyKcal = kcal, EnergyKj = kj,
        EdiblePartPercent = edible
    };

    private static RecipeNutritionLine GramLine(Guid ingredientId, decimal grams, decimal? edible = null, int sortOrder = 1) =>
        Line(ingredientId, grams, Gram, UnitTypeNames.Weight, 1, sortOrder, edible);

    private static List<RecipeNutritionValue> Values(Guid ingredientId) =>
    [
        new() { IngredientId = ingredientId, NutrientDefinitionId = "Fett", Quantity = 10, NutrientSortOrder = 1 },
        new() { IngredientId = ingredientId, NutrientDefinitionId = "Protein", Quantity = 5, NutrientSortOrder = 30 },
        new() { IngredientId = ingredientId, NutrientDefinitionId = "Trans", Quantity = 0, NutrientSortOrder = 2 }
    ];

    private static RecipeNutritionInput Input(int servings, List<RecipeNutritionLine> lines,
        List<RecipeNutritionPortion>? portions = null, List<RecipeNutritionValue>? values = null) => new()
    {
        RecipeId = Guid.NewGuid(), Servings = servings, Lines = lines, Portions = portions ?? [], Values = values ?? []
    };

    [Fact]
    public void Calculate_ScalesPer100GramsByTheGrams_AndDividesByServingsForPerServing()
    {
        var id = Guid.NewGuid();

        var result = RecipeNutritionCalculator.Calculate(Input(4, [GramLine(id, 200)], values: Values(id)));

        Assert.Equal(200m, result.EnergyKcal!.Total);
        Assert.Equal(50m, result.EnergyKcal.PerServing);
        Assert.Equal(836m, result.EnergyKj!.Total);
        var fat = Assert.Single(result.Nutrients, n => n.NutrientId == "Fett");
        Assert.Equal(20m, fat.Total);
        Assert.Equal(5m, fat.PerServing);
        Assert.Equal(1, result.CountedIngredients);
        Assert.Equal(1, result.TotalIngredients);
        Assert.Empty(result.SkippedLines);
    }

    [Fact]
    public void Calculate_ReturnsOnlyNutrientsWithAValue_AndNoZeros()
    {
        var id = Guid.NewGuid();

        var result = RecipeNutritionCalculator.Calculate(Input(1, [GramLine(id, 100)], values: Values(id)));

        Assert.DoesNotContain(result.Nutrients, n => n.NutrientId == "Trans");
        Assert.All(result.Nutrients, n => Assert.True(n.Total > 0));
    }

    [Fact]
    public void Calculate_OrdersTheNutrientsLikeTheNutrientCatalog()
    {
        var id = Guid.NewGuid();
        List<RecipeNutritionValue> values =
        [
            new() { IngredientId = id, NutrientDefinitionId = "Protein", Quantity = 5, NutrientSortOrder = 30 },
            new() { IngredientId = id, NutrientDefinitionId = "Fett", Quantity = 10, NutrientSortOrder = 1 },
            new() { IngredientId = id, NutrientDefinitionId = "Vit C", Quantity = 2, NutrientSortOrder = 42 }
        ];

        var result = RecipeNutritionCalculator.Calculate(Input(1, [GramLine(id, 100)], values: values));

        Assert.Equal(["Fett", "Protein", "Vit C"], result.Nutrients.Select(n => n.NutrientId));
    }

    [Fact]
    public void Calculate_SumsAcrossIngredients()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var values = Values(a).Concat(Values(b)).ToList();

        var result = RecipeNutritionCalculator.Calculate(Input(2, [GramLine(a, 100), GramLine(b, 300, sortOrder: 2)], values: values));

        Assert.Equal(400m, result.EnergyKcal!.Total);
        Assert.Equal(40m, Assert.Single(result.Nutrients, n => n.NutrientId == "Fett").Total);
        Assert.Equal(2, result.CountedIngredients);
    }

    [Fact]
    public void Calculate_RemovesTheInedibleShare_ForAmountsInWeightUnits()
    {
        var id = Guid.NewGuid();

        // 500 g hel (66 % spiselig) = 330 g spiselig -> 330 kcal.
        var result = RecipeNutritionCalculator.Calculate(Input(1, [GramLine(id, 500, edible: 66)], values: Values(id)));

        Assert.Equal(330m, result.EnergyKcal!.Total);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(100)]
    public void Calculate_TreatsUnknownZeroOr100PercentEdibleAsFullyEdible(int? edible)
    {
        var id = Guid.NewGuid();

        var result = RecipeNutritionCalculator.Calculate(Input(1, [GramLine(id, 200, edible)], values: Values(id)));

        Assert.Equal(200m, result.EnergyKcal!.Total);
    }

    [Fact]
    public void Calculate_UsesTheKilogramRatioForWeightUnits()
    {
        var id = Guid.NewGuid();

        var result = RecipeNutritionCalculator.Calculate(Input(1, [Line(id, 2, Kilogram, UnitTypeNames.Weight, 1000)], values: Values(id)));

        Assert.Equal(2000m, result.EnergyKcal!.Total);
    }

    // Porsjonsvekter er allerede vekt av spiselig del - derfor ikke et nytt fratrekk selv om ingrediensen er 66 % spiselig.
    [Fact]
    public void Calculate_UsesThePortionWeightAsEdibleGrams_WithoutRemovingTheInedibleShareAgain()
    {
        var id = Guid.NewGuid();
        RecipeNutritionPortion[] portions =
            [new() { IngredientId = id, UnitId = Piece, UnitTypeName = "antall", UnitBaseRatio = 1, GramsPerPortion = 120 }];

        var result = RecipeNutritionCalculator.Calculate(Input(1,
            [Line(id, 2, Piece, "antall", 1, edible: 66)], portions.ToList(), Values(id)));

        Assert.Equal(240m, result.EnergyKcal!.Total);
    }

    [Fact]
    public void Calculate_PrefersTheIngredientsOwnPortionOverTheUnitsRatio()
    {
        var id = Guid.NewGuid();
        RecipeNutritionPortion[] portions =
            [new() { IngredientId = id, UnitId = Deciliter, UnitTypeName = UnitTypeNames.Volume, UnitBaseRatio = 100, GramsPerPortion = 55 }];

        var result = RecipeNutritionCalculator.Calculate(Input(1,
            [Line(id, 2, Deciliter, UnitTypeNames.Volume, 100)], portions.ToList(), Values(id)));

        Assert.Equal(110m, result.EnergyKcal!.Total);
    }

    // 1 ss (15 ml) uten egen porsjon: skaleres fra dl-vekten (55 g per 100 ml) -> 15 * 0,55 = 8,25 g.
    [Fact]
    public void Calculate_ScalesAVolumeUnitThroughTheLargestVolumePortion_WhenThereIsNoPortionForThatUnit()
    {
        var id = Guid.NewGuid();
        RecipeNutritionPortion[] portions =
            [new() { IngredientId = id, UnitId = Deciliter, UnitTypeName = UnitTypeNames.Volume, UnitBaseRatio = 100, GramsPerPortion = 55 }];

        var result = RecipeNutritionCalculator.Calculate(Input(1,
            [Line(id, 1, Tablespoon, UnitTypeNames.Volume, 15)], portions.ToList(), Values(id)));

        Assert.Equal(8.25m, result.EnergyKcal!.Total);
        Assert.Equal(1, result.CountedIngredients);
    }

    [Fact]
    public void Calculate_SkipsAVolumeLineWithNoVolumePortion_AsNoConversion()
    {
        var id = Guid.NewGuid();

        var result = RecipeNutritionCalculator.Calculate(Input(1,
            [Line(id, 1, Tablespoon, UnitTypeNames.Volume, 15)], values: Values(id)));

        var skipped = Assert.Single(result.SkippedLines);
        Assert.Equal(NutritionSkipReason.NoConversion, skipped.Reason);
        Assert.Null(result.EnergyKcal);
        Assert.Empty(result.Nutrients);
    }

    [Fact]
    public void Calculate_SkipsACountUnitWithoutAPortion_AsNoConversion()
    {
        var id = Guid.NewGuid();

        var result = RecipeNutritionCalculator.Calculate(Input(1, [Line(id, 1, Pinch, "antall", 1)], values: Values(id)));

        Assert.Equal(NutritionSkipReason.NoConversion, Assert.Single(result.SkippedLines).Reason);
    }

    [Fact]
    public void Calculate_SkipsALineWithAmountZero_AsToTaste_AndTreatsItAsNothing()
    {
        var id = Guid.NewGuid();
        var salt = Guid.NewGuid();

        var result = RecipeNutritionCalculator.Calculate(Input(1,
            [GramLine(id, 100), GramLine(salt, 0, sortOrder: 2)], values: Values(id).Concat(Values(salt)).ToList()));

        Assert.Equal(NutritionSkipReason.ToTaste, Assert.Single(result.SkippedLines).Reason);
        Assert.Equal(100m, result.EnergyKcal!.Total);
        Assert.Equal(1, result.CountedIngredients);
        Assert.Equal(2, result.TotalIngredients);
    }

    [Fact]
    public void Calculate_SkipsAnUnconfirmedIngredient_AsUnconfirmed()
    {
        var id = Guid.NewGuid();

        var result = RecipeNutritionCalculator.Calculate(Input(1,
            [GramLine(id, 100), Line(null, 50, Gram, UnitTypeNames.Weight, 1, sortOrder: 2)], values: Values(id)));

        Assert.Equal(NutritionSkipReason.Unconfirmed, Assert.Single(result.SkippedLines).Reason);
        Assert.Equal(100m, result.EnergyKcal!.Total);
    }

    [Fact]
    public void Calculate_ReturnsNoEnergyAndNoNutrients_WhenNothingCouldBeCounted()
    {
        var result = RecipeNutritionCalculator.Calculate(Input(2, [Line(null, 50, Gram, UnitTypeNames.Weight, 1)]));

        Assert.Null(result.EnergyKcal);
        Assert.Null(result.EnergyKj);
        Assert.Empty(result.Nutrients);
        Assert.Equal(0, result.CountedIngredients);
    }

    [Fact]
    public void Calculate_OmitsKilojoulesWhenNoIngredientHasThem()
    {
        var id = Guid.NewGuid();

        var result = RecipeNutritionCalculator.Calculate(Input(1,
            [Line(id, 100, Gram, UnitTypeNames.Weight, 1, kj: null)], values: Values(id)));

        Assert.NotNull(result.EnergyKcal);
        Assert.Null(result.EnergyKj);
    }

    [Fact]
    public void Calculate_ListsSkippedLinesInRecipeOrder()
    {
        var result = RecipeNutritionCalculator.Calculate(Input(1,
        [
            Line(null, 1, Gram, UnitTypeNames.Weight, 1, sortOrder: 3),
            Line(Guid.NewGuid(), 0, Gram, UnitTypeNames.Weight, 1, sortOrder: 1)
        ]));

        Assert.Equal([NutritionSkipReason.ToTaste, NutritionSkipReason.Unconfirmed], result.SkippedLines.Select(s => s.Reason));
    }
}
