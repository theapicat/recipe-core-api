using Application.Caching.Interfaces;
using Application.MediatR.Catalog;
using Domain.Ingredients;
using NSubstitute;
using Persistence.Services;
using Xunit;

namespace Tests.Application.MediatR.Catalog;

public class GetAllCatalogQueryHandlerTests
{
    private class FakeReader(List<IngredientCategory> items) : DbReader<IngredientCategory>("unused")
    {
        public int CallCount { get; private set; }

        public override Task<List<IngredientCategory>> GetAllAsync()
        {
            CallCount++;
            return Task.FromResult(items);
        }
    }

    [Fact]
    public async Task Handle_ReturnsFromReader_AndPopulatesCache_OnCacheMiss()
    {
        var items = new List<IngredientCategory> { new() { Id = Guid.NewGuid(), Name = "Grønnsaker" } };
        var reader = new FakeReader(items);
        var cache = Substitute.For<ICacheService>();
        cache.Get<List<IngredientCategory>>(Arg.Any<string>()).Returns((List<IngredientCategory>?)null);

        var handler = new GetAllCatalogQueryHandler<IngredientCategory>(reader, cache);
        var result = await handler.Handle(new GetAllCatalogQuery<IngredientCategory>(), CancellationToken.None);

        Assert.Same(items, result);
        Assert.Equal(1, reader.CallCount);
        cache.Received(1).Set(Arg.Any<string>(), items, Arg.Any<TimeSpan?>());
    }

    [Fact]
    public async Task Handle_ReturnsFromCache_AndNeverCallsReader_OnCacheHit()
    {
        var cachedItems = new List<IngredientCategory> { new() { Id = Guid.NewGuid(), Name = "Frukt" } };
        var reader = new FakeReader([]);
        var cache = Substitute.For<ICacheService>();
        cache.Get<List<IngredientCategory>>(Arg.Any<string>()).Returns(cachedItems);

        var handler = new GetAllCatalogQueryHandler<IngredientCategory>(reader, cache);
        var result = await handler.Handle(new GetAllCatalogQuery<IngredientCategory>(), CancellationToken.None);

        Assert.Same(cachedItems, result);
        Assert.Equal(0, reader.CallCount);
        cache.DidNotReceive().Set(Arg.Any<string>(), Arg.Any<List<IngredientCategory>>(), Arg.Any<TimeSpan?>());
    }
}
