using Application.MediatR.Admin.Ingredients;
using Application.MediatR.Catalog;
using Domain.Ingredients;
using Domain.Units;
using MediatR;
using NSubstitute;
using Xunit;
using DomainUnit = Domain.Units.Unit;

namespace Tests.Application.MediatR.Admin.Ingredients;

public class IngredientForeignKeyValidatorTests
{
    private static Task<string?> ValidateAsync(IMediator mediator, IngredientRequest request, Guid? existingId = null) =>
        IngredientForeignKeyValidator.ValidateAsync(mediator, request, existingId, CancellationToken.None);

    [Fact]
    public async Task ValidateAsync_ReturnsNull_ForTheValidRequestAndPassthroughMediator()
    {
        var error = await ValidateAsync(IngredientTestData.PassthroughMediator(), IngredientTestData.ValidRequest());

        Assert.Null(error);
    }

    [Fact]
    public async Task ValidateAsync_RejectsAnUnknownCategory()
    {
        var mediator = IngredientTestData.PassthroughMediator();
        var request = IngredientTestData.ValidRequest() with { CategoryId = Guid.NewGuid() };

        Assert.NotNull(await ValidateAsync(mediator, request));
    }

    [Fact]
    public async Task ValidateAsync_RejectsAnUnknownPrimaryUnitType()
    {
        var mediator = IngredientTestData.PassthroughMediator();
        var request = IngredientTestData.ValidRequest() with { PrimaryUnitTypeId = Guid.NewGuid() };

        Assert.NotNull(await ValidateAsync(mediator, request));
    }

    [Fact]
    public async Task ValidateAsync_RejectsAnUnknownDefaultUnit()
    {
        var mediator = IngredientTestData.PassthroughMediator();
        var request = IngredientTestData.ValidRequest() with { DefaultUnitId = Guid.NewGuid() };

        Assert.NotNull(await ValidateAsync(mediator, request));
    }

    [Fact]
    public async Task ValidateAsync_RejectsADefaultUnitThatBelongsToAnotherUnitType()
    {
        var mediator = Substitute.For<IMediator>();
        var wrongTypeId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        mediator.Send(Arg.Any<GetAllCatalogQuery<IngredientCategory>>(), Arg.Any<CancellationToken>())
            .Returns(new List<IngredientCategory> { new() { Id = IngredientTestData.CategoryId, Name = "x" } });
        mediator.Send(Arg.Any<GetAllCatalogQuery<UnitType>>(), Arg.Any<CancellationToken>())
            .Returns(new List<UnitType>
            {
                new() { Id = IngredientTestData.UnitTypeId, Name = "vekt" },
                new() { Id = wrongTypeId, Name = "volum" }
            });
        // Enheten finnes, men hører til volum - ikke ingrediensens valgte vekt.
        mediator.Send(Arg.Any<GetAllCatalogQuery<DomainUnit>>(), Arg.Any<CancellationToken>())
            .Returns(new List<DomainUnit>
            {
                new() { Id = unitId, Name = "desiliter", Abbreviation = "dl", UnitTypeId = wrongTypeId, BaseUnitRatio = 100 }
            });
        var request = IngredientTestData.ValidRequest() with { DefaultUnitId = unitId };

        Assert.NotNull(await ValidateAsync(mediator, request));
    }

    [Fact]
    public async Task ValidateAsync_RejectsAnUnknownAllergen()
    {
        var mediator = IngredientTestData.PassthroughMediator();
        mediator.Send(Arg.Any<GetAllCatalogQuery<Allergen>>(), Arg.Any<CancellationToken>()).Returns(new List<Allergen>());
        var request = IngredientTestData.ValidRequest() with { AllergenIds = [Guid.NewGuid()] };

        Assert.NotNull(await ValidateAsync(mediator, request));
    }

    [Fact]
    public async Task ValidateAsync_RejectsAnUnknownSearchKeyword()
    {
        var mediator = IngredientTestData.PassthroughMediator();
        mediator.Send(Arg.Any<GetAllCatalogQuery<SearchKeyword>>(), Arg.Any<CancellationToken>()).Returns(new List<SearchKeyword>());
        var request = IngredientTestData.ValidRequest() with { SearchKeywordIds = [Guid.NewGuid()] };

        Assert.NotNull(await ValidateAsync(mediator, request));
    }

    [Fact]
    public async Task ValidateAsync_RejectsAnUnknownNutrientDefinition_AndNamesItInTheMessage()
    {
        var mediator = IngredientTestData.PassthroughMediator();
        mediator.Send(Arg.Any<GetAllCatalogQuery<NutrientDefinition>>(), Arg.Any<CancellationToken>())
            .Returns(new List<NutrientDefinition>());
        var request = IngredientTestData.ValidRequest() with
        {
            NutrientValues = [new IngredientNutrientValueRequest { NutrientDefinitionId = "Fett", Quantity = 1 }]
        };

        var error = await ValidateAsync(mediator, request);

        Assert.NotNull(error);
        Assert.Contains("Fett", error);
    }

    [Fact]
    public async Task ValidateAsync_RejectsAPortionWithAnUnknownUnit()
    {
        var mediator = IngredientTestData.PassthroughMediator();
        var request = IngredientTestData.ValidRequest() with
        {
            Portions = [new IngredientPortionRequest { UnitId = Guid.NewGuid(), GramsPerPortion = 10 }]
        };

        Assert.NotNull(await ValidateAsync(mediator, request));
    }

    [Fact]
    public async Task ValidateAsync_RejectsTwoPortionsForTheSameUnit_AndNamesItInTheMessage()
    {
        var mediator = IngredientTestData.PassthroughMediator();
        var request = IngredientTestData.ValidRequest() with
        {
            Portions =
            [
                new IngredientPortionRequest { UnitId = IngredientTestData.UnitId, GramsPerPortion = 10 },
                new IngredientPortionRequest { UnitId = IngredientTestData.UnitId, GramsPerPortion = 20 }
            ]
        };

        var error = await ValidateAsync(mediator, request);

        Assert.NotNull(error);
        Assert.Contains("gram", error);
    }

    [Fact]
    public async Task ValidateAsync_RejectsAnIngredientThatIsAVariantOfItself()
    {
        var mediator = IngredientTestData.PassthroughMediator();
        var id = Guid.NewGuid();
        var request = IngredientTestData.ValidRequest() with { VariantOfIngredientId = id };

        Assert.NotNull(await ValidateAsync(mediator, request, existingId: id));
    }

    [Fact]
    public async Task ValidateAsync_RejectsAMissingBaseIngredient()
    {
        var mediator = IngredientTestData.PassthroughMediator();
        var baseId = Guid.NewGuid();
        mediator.Send(Arg.Is<GetCatalogByIdQuery<Ingredient, Guid>>(q => q.Id == baseId), Arg.Any<CancellationToken>())
            .Returns((Ingredient?)null);
        var request = IngredientTestData.ValidRequest() with { VariantOfIngredientId = baseId };

        Assert.NotNull(await ValidateAsync(mediator, request));
    }

    [Fact]
    public async Task ValidateAsync_AllowsAVariantOfAnExistingBaseIngredient()
    {
        var mediator = IngredientTestData.PassthroughMediator();
        var baseId = Guid.NewGuid();
        var baseIngredient = IngredientMapper.ToIngredient(IngredientTestData.ValidRequest(), baseId, isOfficial: true, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        mediator.Send(Arg.Is<GetCatalogByIdQuery<Ingredient, Guid>>(q => q.Id == baseId), Arg.Any<CancellationToken>())
            .Returns(baseIngredient);
        var request = IngredientTestData.ValidRequest() with { VariantOfIngredientId = baseId };

        Assert.Null(await ValidateAsync(mediator, request));
    }

    [Fact]
    public async Task ValidateAsync_RejectsAVariantChainThatLoopsBackToTheIngredientBeingSaved()
    {
        var mediator = IngredientTestData.PassthroughMediator();
        var ownId = Guid.NewGuid();
        var baseId = Guid.NewGuid();
        // baseId -> ownId -> baseId: en løkke som ikke går via selvreferanse-sjekken direkte.
        var baseIngredient = IngredientMapper.ToIngredient(
            IngredientTestData.ValidRequest() with { VariantOfIngredientId = ownId }, baseId, isOfficial: false, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        mediator.Send(Arg.Is<GetCatalogByIdQuery<Ingredient, Guid>>(q => q.Id == baseId), Arg.Any<CancellationToken>())
            .Returns(baseIngredient);
        var request = IngredientTestData.ValidRequest() with { VariantOfIngredientId = baseId };

        Assert.NotNull(await ValidateAsync(mediator, request, existingId: ownId));
    }
}
