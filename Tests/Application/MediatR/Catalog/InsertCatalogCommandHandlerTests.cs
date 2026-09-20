using Application.Caching.Interfaces;
using Application.MediatR.Catalog;
using Domain.Ingredients;
using NSubstitute;
using Persistence.Services;
using Xunit;

namespace Tests.Application.MediatR.Catalog;

public class InsertCatalogCommandHandlerTests
{
    private class RecordingWriter<T> : DbWriter<T>
    {
        public RecordingWriter() : base("unused")
        {
        }

        public T? Inserted { get; private set; }

        public override Task AddAsync(T entity)
        {
            Inserted = entity;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Handle_AssignsServerGeneratedGuid_IgnoringClientId_AndInvalidatesCache()
    {
        var clientId = Guid.NewGuid();
        var entity = new IngredientCategory { Id = clientId, Name = "Nøtter" };
        var writer = new RecordingWriter<IngredientCategory>();
        var cache = Substitute.For<ICacheService>();
        var handler = new InsertCatalogCommandHandler<IngredientCategory, Guid>(writer, cache);

        var id = await handler.Handle(new InsertCatalogCommand<IngredientCategory, Guid>(entity), CancellationToken.None);

        Assert.NotEqual(Guid.Empty, id);
        Assert.NotEqual(clientId, id);
        Assert.Same(entity, writer.Inserted);
        Assert.Equal(id, writer.Inserted!.Id);
        cache.Received(1).Remove(Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_KeepsCallerSuppliedId_ForNonGuidKeys()
    {
        var entity = new NutrientDefinition { Id = "Vit C", Name = "Vitamin C", Unit = "mg", DecimalPrecision = 1 };
        var writer = new RecordingWriter<NutrientDefinition>();
        var handler = new InsertCatalogCommandHandler<NutrientDefinition, string>(writer, Substitute.For<ICacheService>());

        var id = await handler.Handle(new InsertCatalogCommand<NutrientDefinition, string>(entity), CancellationToken.None);

        Assert.Equal("Vit C", id);
        Assert.Equal("Vit C", writer.Inserted!.Id);
    }
}
