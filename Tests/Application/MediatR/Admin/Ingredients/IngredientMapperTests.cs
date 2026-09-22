using Application.MediatR.Admin.Ingredients;
using Domain.Ingredients;
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
    public void Validate_RejectsNegativeKilojoules()
    {
        Assert.NotNull(IngredientMapper.Validate(IngredientTestData.ValidRequest() with { EnergyKj = -1 }));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100.01)]
    [InlineData(-5)]
    public void Validate_RejectsAnEdiblePartPercentOutsideZeroToOneHundred(decimal percent)
    {
        Assert.NotNull(IngredientMapper.Validate(IngredientTestData.ValidRequest() with { EdiblePartPercent = percent }));
    }

    [Fact]
    public void Validate_AllowsAnEdiblePartPercentOfOneHundred()
    {
        Assert.Null(IngredientMapper.Validate(IngredientTestData.ValidRequest() with { EdiblePartPercent = 100 }));
    }

    [Fact]
    public void Validate_RejectsEnergyThatOverflowsTheNumericColumn()
    {
        // numeric(10,2): maks 99 999 999,99.
        Assert.NotNull(IngredientMapper.Validate(IngredientTestData.ValidRequest() with { EnergyKcal = 100_000_000m }));
        Assert.NotNull(IngredientMapper.Validate(IngredientTestData.ValidRequest() with { EnergyKj = 100_000_000m }));
    }

    [Fact]
    public void Validate_AllowsEnergyAtTheNumericColumnBoundary()
    {
        Assert.Null(IngredientMapper.Validate(IngredientTestData.ValidRequest() with { EnergyKcal = 99_999_999.99m }));
    }

    [Fact]
    public void Validate_RejectsANutrientQuantityThatOverflowsTheNumericColumn()
    {
        var request = IngredientTestData.ValidRequest() with
        {
            // numeric(12,4): maks 99 999 999,9999.
            NutrientValues = [new IngredientNutrientValueRequest { NutrientDefinitionId = "Fett", Quantity = 100_000_000m }]
        };

        Assert.NotNull(IngredientMapper.Validate(request));
    }

    [Fact]
    public void Validate_RejectsAPortionThatOverflowsTheNumericColumn()
    {
        var request = IngredientTestData.ValidRequest() with
        {
            // numeric(10,2): maks 99 999 999,99.
            Portions = [new IngredientPortionRequest { UnitId = Guid.NewGuid(), GramsPerPortion = 100_000_000m }]
        };

        Assert.NotNull(IngredientMapper.Validate(request));
    }

    [Fact]
    public void Validate_RejectsADuplicateNutrientDefinitionId()
    {
        var request = IngredientTestData.ValidRequest() with
        {
            NutrientValues =
            [
                new IngredientNutrientValueRequest { NutrientDefinitionId = "Fett", Quantity = 1 },
                new IngredientNutrientValueRequest { NutrientDefinitionId = "Fett", Quantity = 2 }
            ]
        };

        var error = IngredientMapper.Validate(request);

        Assert.NotNull(error);
        Assert.Contains("Fett", error);
    }

    [Theory]
    [InlineData("ikke en adresse")]
    [InlineData("ftp://example.test/kilde")]
    [InlineData("javascript:alert(1)")]
    public void Validate_RejectsASourceUrlThatIsNotHttpOrHttps(string url)
    {
        Assert.NotNull(IngredientMapper.Validate(IngredientTestData.ValidRequest() with { SourceUrl = url }));
    }

    [Fact]
    public void Validate_AllowsAnHttpsSourceUrl()
    {
        Assert.Null(IngredientMapper.Validate(IngredientTestData.ValidRequest() with { SourceUrl = "https://example.test/kilde" }));
    }

    [Fact]
    public void Validate_RejectsVerifiedWithNoNutrientValues()
    {
        var error = IngredientMapper.Validate(IngredientTestData.ValidRequest() with { IsVerified = true, NutrientValues = [] });

        Assert.NotNull(error);
    }

    [Fact]
    public void Validate_AllowsVerifiedWhenAtLeastOneNutrientValueIsPresent()
    {
        var request = IngredientTestData.ValidRequest() with
        {
            IsVerified = true,
            NutrientValues = [new IngredientNutrientValueRequest { NutrientDefinitionId = "Fett", Quantity = 1 }]
        };

        Assert.Null(IngredientMapper.Validate(request));
    }

    private static Ingredient ExistingOfficial() => IngredientMapper.ToIngredient(
        IngredientTestData.ValidRequest() with
        {
            SourceId = "06.010", SourceUrl = "https://www.matvaretabellen.no/agurk-ra/",
            NutrientValues = [new IngredientNutrientValueRequest { NutrientDefinitionId = "Fett", Quantity = 0.1m, SourceId = "10" }]
        },
        Guid.NewGuid(), isOfficial: true, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

    [Fact]
    public void ValidateOfficialLock_AllowsAnUnchangedRequest()
    {
        var existing = ExistingOfficial();
        var request = IngredientTestData.ValidRequest() with
        {
            SourceId = existing.SourceId, SourceUrl = existing.SourceUrl,
            NutrientValues = [new IngredientNutrientValueRequest { NutrientDefinitionId = "Fett", Quantity = 0.1m, SourceId = "10" }]
        };

        Assert.Null(IngredientMapper.ValidateOfficialLock(existing, request));
    }

    [Fact]
    public void ValidateOfficialLock_AllowsANameChangeThatIsOnlyCapitalization()
    {
        var existing = ExistingOfficial();
        var request = IngredientTestData.ValidRequest() with
        {
            Name = "Agurk", SourceId = existing.SourceId, SourceUrl = existing.SourceUrl,
            NutrientValues = [new IngredientNutrientValueRequest { NutrientDefinitionId = "Fett", Quantity = 0.1m, SourceId = "10" }]
        };

        Assert.Null(IngredientMapper.ValidateOfficialLock(existing, request));
    }

    [Fact]
    public void ValidateOfficialLock_AllowsAllergenCategoryUnitAndPortionChanges()
    {
        var existing = ExistingOfficial();
        var request = IngredientTestData.ValidRequest() with
        {
            CategoryId = Guid.NewGuid(), PrimaryUnitTypeId = Guid.NewGuid(), DefaultUnitId = Guid.NewGuid(),
            AllergenIds = [Guid.NewGuid()], SearchKeywordIds = [Guid.NewGuid()], IsVerified = false,
            Portions = [new IngredientPortionRequest { UnitId = Guid.NewGuid(), GramsPerPortion = 50 }],
            SourceId = existing.SourceId, SourceUrl = existing.SourceUrl,
            NutrientValues = [new IngredientNutrientValueRequest { NutrientDefinitionId = "Fett", Quantity = 0.1m, SourceId = "10" }]
        };

        Assert.Null(IngredientMapper.ValidateOfficialLock(existing, request));
    }

    [Fact]
    public void ValidateOfficialLock_AllowsChangingAllergensReviewed()
    {
        var existing = ExistingOfficial();
        var request = IngredientTestData.ValidRequest() with
        {
            SourceId = existing.SourceId, SourceUrl = existing.SourceUrl,
            NutrientValues = [new IngredientNutrientValueRequest { NutrientDefinitionId = "Fett", Quantity = 0.1m, SourceId = "10" }],
            AllergensReviewed = true
        };

        Assert.Null(IngredientMapper.ValidateOfficialLock(existing, request));
    }

    [Fact]
    public void ValidateOfficialLock_AllowsResendingNutrientValuesInADifferentOrderWithNewRowIds()
    {
        var existing = IngredientMapper.ToIngredient(
            IngredientTestData.ValidRequest() with
            {
                NutrientValues =
                [
                    new IngredientNutrientValueRequest { NutrientDefinitionId = "Fett", Quantity = 1 },
                    new IngredientNutrientValueRequest { NutrientDefinitionId = "Vann", Quantity = 2 }
                ]
            },
            Guid.NewGuid(), isOfficial: true, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        var request = IngredientTestData.ValidRequest() with
        {
            NutrientValues =
            [
                new IngredientNutrientValueRequest { NutrientDefinitionId = "Vann", Quantity = 2 },
                new IngredientNutrientValueRequest { NutrientDefinitionId = "Fett", Quantity = 1 }
            ]
        };

        Assert.Null(IngredientMapper.ValidateOfficialLock(existing, request));
    }

    [Theory]
    [InlineData("Gulrot")]
    public void ValidateOfficialLock_RejectsANameChangeBeyondCapitalization(string newName)
    {
        var existing = ExistingOfficial();
        var request = IngredientTestData.ValidRequest() with
        {
            Name = newName, SourceId = existing.SourceId, SourceUrl = existing.SourceUrl,
            NutrientValues = [new IngredientNutrientValueRequest { NutrientDefinitionId = "Fett", Quantity = 0.1m, SourceId = "10" }]
        };

        Assert.NotNull(IngredientMapper.ValidateOfficialLock(existing, request));
    }

    [Fact]
    public void ValidateOfficialLock_RejectsAChangedEnergyValue()
    {
        var existing = ExistingOfficial();
        var request = IngredientTestData.ValidRequest() with { EnergyKcal = existing.EnergyKcal + 1 };

        Assert.NotNull(IngredientMapper.ValidateOfficialLock(existing, request));
    }

    [Fact]
    public void ValidateOfficialLock_RejectsAChangedEdiblePartPercent()
    {
        var existing = ExistingOfficial();
        var request = IngredientTestData.ValidRequest() with { EdiblePartPercent = 42 };

        Assert.NotNull(IngredientMapper.ValidateOfficialLock(existing, request));
    }

    [Fact]
    public void ValidateOfficialLock_RejectsAChangedSourceId()
    {
        var existing = ExistingOfficial();
        var request = IngredientTestData.ValidRequest() with { SourceId = "99.999" };

        Assert.NotNull(IngredientMapper.ValidateOfficialLock(existing, request));
    }

    [Fact]
    public void ValidateOfficialLock_RejectsAnAddedNutrientValue()
    {
        var existing = ExistingOfficial();
        var request = IngredientTestData.ValidRequest() with
        {
            SourceId = existing.SourceId, SourceUrl = existing.SourceUrl,
            NutrientValues =
            [
                new IngredientNutrientValueRequest { NutrientDefinitionId = "Fett", Quantity = 0.1m, SourceId = "10" },
                new IngredientNutrientValueRequest { NutrientDefinitionId = "Vann", Quantity = 96m }
            ]
        };

        Assert.NotNull(IngredientMapper.ValidateOfficialLock(existing, request));
    }

    [Fact]
    public void ValidateOfficialLock_RejectsAChangedNutrientQuantity()
    {
        var existing = ExistingOfficial();
        var request = IngredientTestData.ValidRequest() with
        {
            SourceId = existing.SourceId, SourceUrl = existing.SourceUrl,
            NutrientValues = [new IngredientNutrientValueRequest { NutrientDefinitionId = "Fett", Quantity = 999m, SourceId = "10" }]
        };

        Assert.NotNull(IngredientMapper.ValidateOfficialLock(existing, request));
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

        var ingredient = IngredientMapper.ToIngredient(request, id, isOfficial: false, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

        Assert.Equal(id, ingredient.Id);
        Assert.Equal("agurk", ingredient.Name);
        Assert.Single(ingredient.AllergenIds);
        Assert.All(ingredient.NutrientValues, v => Assert.Equal(id, v.IngredientId));
        Assert.All(ingredient.Portions, p => Assert.Equal(id, p.IngredientId));
        Assert.Equal(3, ingredient.NutrientValues.Select(v => v.Id).Concat(ingredient.Portions.Select(p => p.Id)).Distinct().Count());
        Assert.DoesNotContain(Guid.Empty, ingredient.NutrientValues.Select(v => v.Id));
    }
}
