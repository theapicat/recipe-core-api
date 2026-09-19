using Application.Caching.Interfaces;
using Application.MediatR.Catalog;
using Domain.Ingredients;
using NSubstitute;
using Persistence.Services;
using Xunit;

namespace Tests.Application.MediatR.Catalog;

public class InsertCatalogCommandHandlerTests
{
    private class RecordingWriter : DbWriter<IngredientCategory>
    {
        public RecordingWriter() : base("unused")
        {
        }

        public IngredientCategory? Inserted { get; private set; }

        public override Task AddAsync(IngredientCategory entity)
        {
            Inserted = entity;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Handle_InsertsEntity_AndInvalidatesCache()
    {
        var entity = new IngredientCategory { Id = Guid.NewGuid(), Name = "Nøtter" };
        var writer = new RecordingWriter();
        var cache = Substitute.For<ICacheService>();
        var handler = new InsertCatalogCommandHandler<IngredientCategory>(writer, cache);

        var result = await handler.Handle(new InsertCatalogCommand<IngredientCategory>(entity), CancellationToken.None);

        Assert.True(result);
        Assert.Same(entity, writer.Inserted);
        cache.Received(1).Remove(Arg.Any<string>());
    }
}
