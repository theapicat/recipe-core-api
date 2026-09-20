using Application.Caching.Interfaces;
using Application.MediatR.Catalog;
using Domain.Ingredients;
using NSubstitute;
using Persistence.Services;
using Xunit;

namespace Tests.Application.MediatR.Catalog;

public class UpdateCatalogCommandHandlerTests
{
    private class RecordingWriter : DbWriter<IngredientCategory>
    {
        public RecordingWriter() : base("unused")
        {
        }

        public IngredientCategory? Updated { get; private set; }

        public override Task UpdateAsync(IngredientCategory entity)
        {
            Updated = entity;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Handle_UpdatesEntity_AndInvalidatesCache()
    {
        var entity = new IngredientCategory { Id = Guid.NewGuid(), Name = "Nøtter" };
        var writer = new RecordingWriter();
        var cache = Substitute.For<ICacheService>();
        var handler = new UpdateCatalogCommandHandler<IngredientCategory>(writer, cache);

        var result = await handler.Handle(new UpdateCatalogCommand<IngredientCategory>(entity), CancellationToken.None);

        Assert.True(result);
        Assert.Same(entity, writer.Updated);
        cache.Received(1).Remove(Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_StoresTheNameLowercaseAndTrimmed()
    {
        var entity = new IngredientCategory { Id = Guid.NewGuid(), Name = " Nøtter OG frø " };
        var writer = new RecordingWriter();
        var handler = new UpdateCatalogCommandHandler<IngredientCategory>(writer, Substitute.For<ICacheService>());

        await handler.Handle(new UpdateCatalogCommand<IngredientCategory>(entity), CancellationToken.None);

        Assert.Equal("nøtter og frø", writer.Updated!.Name);
    }
}
