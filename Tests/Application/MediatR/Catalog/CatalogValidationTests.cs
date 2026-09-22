using Application.MediatR.Catalog;
using MediatR;
using NSubstitute;
using Xunit;
using DomainUnit = Domain.Units.Unit;
using DomainUnitType = Domain.Units.UnitType;

namespace Tests.Application.MediatR.Catalog;

public class CatalogValidationTests
{
    private static IMediator MediatorReturning(DomainUnitType? unitType)
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetAllCatalogQuery<DomainUnitType>>(), Arg.Any<CancellationToken>())
            .Returns(unitType is null ? [] : new List<DomainUnitType> { unitType });
        return mediator;
    }

    private static DomainUnit Unit(Guid unitTypeId, string abbreviation = "ss", decimal baseUnitRatio = 15) => new()
    {
        Name = "spiseskje", Abbreviation = abbreviation, UnitTypeId = unitTypeId, BaseUnitRatio = baseUnitRatio
    };

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ValidateAsync_RejectsABlankAbbreviation(string abbreviation)
    {
        var unitTypeId = Guid.NewGuid();
        var unit = Unit(unitTypeId, abbreviation: abbreviation);

        var error = await CatalogValidation.ValidateAsync(
            MediatorReturning(new DomainUnitType { Id = unitTypeId, Name = "volum" }), unit, CancellationToken.None);

        Assert.NotNull(error);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ValidateAsync_RejectsARatioThatIsNotPositive(decimal ratio)
    {
        var unitTypeId = Guid.NewGuid();
        var unit = Unit(unitTypeId, baseUnitRatio: ratio);

        var error = await CatalogValidation.ValidateAsync(
            MediatorReturning(new DomainUnitType { Id = unitTypeId, Name = "volum" }), unit, CancellationToken.None);

        Assert.NotNull(error);
    }

    [Fact]
    public async Task ValidateAsync_RejectsAnUnknownUnitType()
    {
        var unit = Unit(Guid.NewGuid());

        var error = await CatalogValidation.ValidateAsync(MediatorReturning(null), unit, CancellationToken.None);

        Assert.NotNull(error);
    }

    [Fact]
    public async Task ValidateAsync_RejectsACountUnitWithARatioOtherThanOne()
    {
        var unitTypeId = Guid.NewGuid();
        var unit = Unit(unitTypeId, baseUnitRatio: 2);

        var error = await CatalogValidation.ValidateAsync(
            MediatorReturning(new DomainUnitType { Id = unitTypeId, Name = "antall" }), unit, CancellationToken.None);

        Assert.NotNull(error);
    }

    [Fact]
    public async Task ValidateAsync_AllowsACountUnitWithARatioOfOne()
    {
        var unitTypeId = Guid.NewGuid();
        var unit = Unit(unitTypeId, baseUnitRatio: 1);

        var error = await CatalogValidation.ValidateAsync(
            MediatorReturning(new DomainUnitType { Id = unitTypeId, Name = "antall" }), unit, CancellationToken.None);

        Assert.Null(error);
    }

    [Fact]
    public async Task ValidateAsync_AllowsAValidNonCountUnit()
    {
        var unitTypeId = Guid.NewGuid();
        var unit = Unit(unitTypeId);

        var error = await CatalogValidation.ValidateAsync(
            MediatorReturning(new DomainUnitType { Id = unitTypeId, Name = "volum" }), unit, CancellationToken.None);

        Assert.Null(error);
    }
}
