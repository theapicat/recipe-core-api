using Application.Caching.Interfaces;
using Application.MediatR.Catalog;
using Domain;
using Domain.Ingredients;
using NSubstitute;
using Persistence.Services;
using Xunit;
using DomainUnit = Domain.Units.Unit;

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

    private class TextKeyEntity : IHasId<string>
    {
        public required string Id { get; set; }
    }

    [Fact]
    public async Task Handle_KeepsCallerSuppliedId_ForNonGuidKeys()
    {
        var entity = new TextKeyEntity { Id = "Vit C" };
        var writer = new RecordingWriter<TextKeyEntity>();
        var handler = new InsertCatalogCommandHandler<TextKeyEntity, string>(writer, Substitute.For<ICacheService>());

        var id = await handler.Handle(new InsertCatalogCommand<TextKeyEntity, string>(entity), CancellationToken.None);

        Assert.Equal("Vit C", id);
        Assert.Equal("Vit C", writer.Inserted!.Id);
    }

    [Fact]
    public async Task Handle_StoresTheNameLowercaseAndTrimmed()
    {
        var entity = new IngredientCategory { Name = "  Nøtter   OG Frø " };
        var writer = new RecordingWriter<IngredientCategory>();
        var handler = new InsertCatalogCommandHandler<IngredientCategory, Guid>(writer, Substitute.For<ICacheService>());

        await handler.Handle(new InsertCatalogCommand<IngredientCategory, Guid>(entity), CancellationToken.None);

        Assert.Equal("nøtter og frø", writer.Inserted!.Name);
    }

    [Fact]
    public async Task Handle_TrimsTheUnitAbbreviationButKeepsItsCase()
    {
        var entity = new DomainUnit { Name = "Spiseskje", Abbreviation = " mg-ATE ", UnitTypeId = Guid.NewGuid(), BaseUnitRatio = 15 };
        var writer = new RecordingWriter<DomainUnit>();
        var handler = new InsertCatalogCommandHandler<DomainUnit, Guid>(writer, Substitute.For<ICacheService>());

        await handler.Handle(new InsertCatalogCommand<DomainUnit, Guid>(entity), CancellationToken.None);

        Assert.Equal("spiseskje", writer.Inserted!.Name);
        Assert.Equal("mg-ATE", writer.Inserted.Abbreviation);
    }
}
