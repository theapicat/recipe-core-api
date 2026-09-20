using Application.Caching.Interfaces;
using Application.MediatR.Catalog;
using Domain.Ingredients;
using NSubstitute;
using Persistence.Services;
using Xunit;

namespace Tests.Application.MediatR.Catalog;

public class DeleteCatalogCommandHandlerTests
{
    private class FakeWriter(int affectedRows) : DbWriter<IngredientCategory>("unused")
    {
        public override Task<int> DeleteAsync<TId>(TId id) => Task.FromResult(affectedRows);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(0, false)]
    public async Task Handle_ReturnsWhetherARowWasActuallyDeleted(int affectedRows, bool expected)
    {
        var writer = new FakeWriter(affectedRows);
        var cache = Substitute.For<ICacheService>();
        var handler = new DeleteCatalogCommandHandler<IngredientCategory, Guid>(writer, cache);

        var result = await handler.Handle(new DeleteCatalogCommand<IngredientCategory, Guid>(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(expected, result);
        cache.Received(1).Remove(Arg.Any<string>());
    }
}
