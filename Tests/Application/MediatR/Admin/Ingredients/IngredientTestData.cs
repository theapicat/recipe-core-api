using Application.MediatR.Admin.Ingredients;
using Domain.Ingredients;
using Persistence.Services;

namespace Tests.Application.MediatR.Admin.Ingredients;

internal static class IngredientTestData
{
    public static IngredientRequest ValidRequest() => new()
    {
        Name = "Agurk",
        CategoryId = Guid.NewGuid(),
        PrimaryUnitTypeId = Guid.NewGuid(),
        DefaultUnitId = Guid.NewGuid(),
        EnergyKcal = 12
    };

    public const string IngredientListCacheKey = "catalog:IngredientListItem:all";
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
