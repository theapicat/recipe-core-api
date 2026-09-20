using Application.MediatR.User.Recipes;
using Application.Results;
using Domain.Recipes;
using NSubstitute;
using Persistence.Interfaces;
using Xunit;

namespace Tests.Application.MediatR.User.Recipes;

public class GetRecipeNutritionQueryHandlerTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly IRecipeReader _reader = Substitute.For<IRecipeReader>();

    // Eier sendes alltid med til readeren, som filtrerer på den - en annen brukers oppskrift er identisk med en som ikke finnes.
    [Fact]
    public async Task Handle_ReturnsNotFound_WhenTheReaderFindsNoRecipe()
    {
        _reader.GetNutritionInputAsync(Arg.Any<Guid>(), _userId).Returns((RecipeNutritionInput?)null);

        var result = await new GetRecipeNutritionQueryHandler(_reader)
            .Handle(new GetRecipeNutritionQuery(_userId, Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(ResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task Handle_CalculatesFromTheInputForTheOwner()
    {
        var recipeId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();
        _reader.GetNutritionInputAsync(recipeId, _userId).Returns(new RecipeNutritionInput
        {
            RecipeId = recipeId, Servings = 2, Portions = [],
            Values = [new RecipeNutritionValue { IngredientId = ingredientId, NutrientDefinitionId = "Fett", Quantity = 10, NutrientSortOrder = 1 }],
            Lines =
            [
                new RecipeNutritionLine
                {
                    LineId = Guid.NewGuid(), SortOrder = 1, Name = "smør", IngredientId = ingredientId, Amount = 100, UnitId = Guid.NewGuid(),
                    UnitTypeName = "vekt", UnitBaseRatio = 1, EnergyKcal = 740
                }
            ]
        });

        var result = await new GetRecipeNutritionQueryHandler(_reader)
            .Handle(new GetRecipeNutritionQuery(_userId, recipeId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(recipeId, result.Value!.RecipeId);
        Assert.Equal(740m, result.Value.EnergyKcal!.Total);
        Assert.Equal(370m, result.Value.EnergyKcal.PerServing);
    }
}
