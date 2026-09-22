using Application.MediatR.Admin.Ingredients;
using Application.MediatR.Catalog;
using Domain.Ingredients;
using Domain.Units;
using MediatR;
using NSubstitute;
using Persistence.Services;
using DomainUnit = Domain.Units.Unit;

namespace Tests.Application.MediatR.Admin.Ingredients;

internal static class IngredientTestData
{
    // Faste id-er (ikke Guid.NewGuid()) slik at PassthroughMediator() kan la fremmednøkkel-sjekkene gå gjennom uten å måtte
    // fange opp hvilken id et bestemt ValidRequest()-kall genererte.
    public static readonly Guid CategoryId = Guid.NewGuid();
    public static readonly Guid UnitTypeId = Guid.NewGuid();
    public static readonly Guid UnitId = Guid.NewGuid();

    public static IngredientRequest ValidRequest() => new()
    {
        Name = "Agurk",
        CategoryId = CategoryId,
        PrimaryUnitTypeId = UnitTypeId,
        DefaultUnitId = UnitId,
        EnergyKcal = 12
    };

    public const string IngredientListCacheKey = "catalog:IngredientListItem:all";

    // En IMediator der IngredientForeignKeyValidator finner alt den leter etter for ValidRequest() (kategori, enhetstype,
    // standardenhet med riktig enhetstype). ValidRequest() har ingen allergener/søkeord/næringsstoffer/porsjoner/variant,
    // så validatoren spør aldri om dem - de trenger ikke stubbes.
    public static IMediator PassthroughMediator()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetAllCatalogQuery<IngredientCategory>>(), Arg.Any<CancellationToken>())
            .Returns(new List<IngredientCategory> { new() { Id = CategoryId, Name = "grønnsaker" } });
        mediator.Send(Arg.Any<GetAllCatalogQuery<UnitType>>(), Arg.Any<CancellationToken>())
            .Returns(new List<UnitType> { new() { Id = UnitTypeId, Name = "vekt" } });
        mediator.Send(Arg.Any<GetAllCatalogQuery<DomainUnit>>(), Arg.Any<CancellationToken>())
            .Returns(new List<DomainUnit>
            {
                new() { Id = UnitId, Name = "gram", Abbreviation = "g", UnitTypeId = UnitTypeId, BaseUnitRatio = 1 }
            });
        return mediator;
    }
}

internal class FakeIngredientWriter(int deletedRows = 1) : DbWriter<Ingredient>("unused")
{
    public Ingredient? Added { get; private set; }
    public Ingredient? Updated { get; private set; }

    public override Task AddAsync(Ingredient entity)
    {
        Added = entity;
        return Task.CompletedTask;
    }

    public override Task UpdateAsync(Ingredient entity)
    {
        Updated = entity;
        return Task.CompletedTask;
    }

    public override Task<int> DeleteAsync<TId>(TId id) => Task.FromResult(deletedRows);
}

internal class FakeIngredientReader(Ingredient? existing) : DbReader<Ingredient>("unused")
{
    public override Task<Ingredient?> GetByIdAsync<TId>(TId id) => Task.FromResult(existing);
}
