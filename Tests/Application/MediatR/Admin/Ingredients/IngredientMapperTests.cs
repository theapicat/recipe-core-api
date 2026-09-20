using Application.MediatR.Admin.Ingredients;
using Xunit;

namespace Tests.Application.MediatR.Admin.Ingredients;

public class IngredientMapperTests
{
    [Fact]
    public void Validate_ReturnsNull_ForAValidRequest()
    {
        Assert.Null(IngredientMapper.Validate(IngredientTestData.ValidRequest()));
    }

    [Fact]
    public void Validate_RejectsBlankName()
    {
        Assert.NotNull(IngredientMapper.Validate(IngredientTestData.ValidRequest() with { Name = "  " }));
    }

    [Fact]
    public void Validate_RejectsNegativeEnergy()
    {
        Assert.NotNull(IngredientMapper.Validate(IngredientTestData.ValidRequest() with { EnergyKcal = -1 }));
    }

    [Fact]
    public void Validate_RejectsNegativeNutrientValue()
    {
        var request = IngredientTestData.ValidRequest() with
        {
            NutrientValues = [new IngredientNutrientValueRequest { NutrientDefinitionId = "Fett", Quantity = -0.1m }]
        };

        Assert.NotNull(IngredientMapper.Validate(request));
    }

    [Fact]
    public void Validate_RejectsPortionWithoutGrams()
    {
        var request = IngredientTestData.ValidRequest() with
        {
            Portions = [new IngredientPortionRequest { UnitId = Guid.NewGuid(), GramsPerPortion = 0 }]
        };

        Assert.NotNull(IngredientMapper.Validate(request));
    }

    [Fact]
    public void ToIngredient_AssignsIdsToIngredientAndEveryChild_AndDeduplicatesLinks()
    {
        var allergenId = Guid.NewGuid();
        var request = IngredientTestData.ValidRequest() with
        {
            Name = "  Agurk  ",
            AllergenIds = [allergenId, allergenId],
            SearchKeywordIds = [Guid.NewGuid()],
            NutrientValues =
            [
                new IngredientNutrientValueRequest { NutrientDefinitionId = "Fett", Quantity = 0.1m },
                new IngredientNutrientValueRequest { NutrientDefinitionId = "Vann", Quantity = 96m }
            ],
            Portions = [new IngredientPortionRequest { UnitId = Guid.NewGuid(), GramsPerPortion = 85 }]
        };
        var id = Guid.NewGuid();

        var ingredient = IngredientMapper.ToIngredient(request, id);

        Assert.Equal(id, ingredient.Id);
        Assert.Equal("agurk", ingredient.Name);
        Assert.Single(ingredient.AllergenIds);
        Assert.All(ingredient.NutrientValues, v => Assert.Equal(id, v.IngredientId));
        Assert.All(ingredient.Portions, p => Assert.Equal(id, p.IngredientId));
        Assert.Equal(3, ingredient.NutrientValues.Select(v => v.Id).Concat(ingredient.Portions.Select(p => p.Id)).Distinct().Count());
        Assert.DoesNotContain(Guid.Empty, ingredient.NutrientValues.Select(v => v.Id));
    }
}
