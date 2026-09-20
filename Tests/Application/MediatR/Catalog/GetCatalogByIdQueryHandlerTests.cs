using Application.MediatR.Catalog;
using Domain.Ingredients;
using Persistence.Services;
using Xunit;

namespace Tests.Application.MediatR.Catalog;

public class GetCatalogByIdQueryHandlerTests
{
    private class FakeReader(IngredientCategory? result) : DbReader<IngredientCategory>("unused")
    {
        public int CallCount { get; private set; }

        public override Task<IngredientCategory?> GetByIdAsync<TId>(TId id)
        {
            CallCount++;
            return Task.FromResult(result);
        }
    }

    [Fact]
    public async Task Handle_ReturnsReaderResult_AlwaysGoingToTheDataSource()
    {
        var expected = new IngredientCategory { Id = Guid.NewGuid(), Name = "Meieri" };
        var reader = new FakeReader(expected);
        var handler = new GetCatalogByIdQueryHandler<IngredientCategory, Guid>(reader);

        var result = await handler.Handle(new GetCatalogByIdQuery<IngredientCategory, Guid>(expected.Id), CancellationToken.None);

        Assert.Same(expected, result);
        Assert.Equal(1, reader.CallCount);
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenNotFound()
    {
        var reader = new FakeReader(null);
        var handler = new GetCatalogByIdQueryHandler<IngredientCategory, Guid>(reader);

        var result = await handler.Handle(new GetCatalogByIdQuery<IngredientCategory, Guid>(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
    }
}
