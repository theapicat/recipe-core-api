using Application.Caching.Interfaces;
using Application.MediatR.Catalog;
using Application.Results;
using Domain;
using Domain.Ingredients;
using MediatR;
using NSubstitute;
using Persistence.Services;
using Xunit;
using DomainUnit = Domain.Units.Unit;
using DomainUnitType = Domain.Units.UnitType;

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

    private static IMediator UnusedMediator() => Substitute.For<IMediator>();

    [Fact]
    public async Task Handle_AssignsServerGeneratedGuid_IgnoringClientId_AndInvalidatesCache()
    {
        var clientId = Guid.NewGuid();
        var entity = new IngredientCategory { Id = clientId, Name = "Nøtter" };
        var writer = new RecordingWriter<IngredientCategory>();
        var cache = Substitute.For<ICacheService>();
        var handler = new InsertCatalogCommandHandler<IngredientCategory, Guid>(writer, cache, UnusedMediator());

        var result = await handler.Handle(new InsertCatalogCommand<IngredientCategory, Guid>(entity), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
        Assert.NotEqual(clientId, result.Value);
        Assert.Same(entity, writer.Inserted);
        Assert.Equal(result.Value, writer.Inserted!.Id);
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
        var handler = new InsertCatalogCommandHandler<TextKeyEntity, string>(writer, Substitute.For<ICacheService>(), UnusedMediator());

        var result = await handler.Handle(new InsertCatalogCommand<TextKeyEntity, string>(entity), CancellationToken.None);

        Assert.Equal("Vit C", result.Value);
        Assert.Equal("Vit C", writer.Inserted!.Id);
    }

    [Fact]
    public async Task Handle_StoresTheNameLowercaseAndTrimmed()
    {
        var entity = new IngredientCategory { Name = "  Nøtter   OG Frø " };
        var writer = new RecordingWriter<IngredientCategory>();
        var handler = new InsertCatalogCommandHandler<IngredientCategory, Guid>(writer, Substitute.For<ICacheService>(), UnusedMediator());

        await handler.Handle(new InsertCatalogCommand<IngredientCategory, Guid>(entity), CancellationToken.None);

        Assert.Equal("nøtter og frø", writer.Inserted!.Name);
    }

    [Fact]
    public async Task Handle_TrimsTheUnitAbbreviationButKeepsItsCase()
    {
        var unitTypeId = Guid.NewGuid();
        var entity = new DomainUnit { Name = "Spiseskje", Abbreviation = " mg-ATE ", UnitTypeId = unitTypeId, BaseUnitRatio = 15 };
        var writer = new RecordingWriter<DomainUnit>();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetAllCatalogQuery<DomainUnitType>>(), Arg.Any<CancellationToken>())
            .Returns([new DomainUnitType { Id = unitTypeId, Name = "volum" }]);
        var handler = new InsertCatalogCommandHandler<DomainUnit, Guid>(writer, Substitute.For<ICacheService>(), mediator);

        await handler.Handle(new InsertCatalogCommand<DomainUnit, Guid>(entity), CancellationToken.None);

        Assert.Equal("spiseskje", writer.Inserted!.Name);
        Assert.Equal("mg-ATE", writer.Inserted.Abbreviation);
    }

    [Fact]
    public async Task Handle_ResetsIsSystemAndUsageCount_IgnoringWhatTheClientSent()
    {
        var entity = new IngredientCategory { Name = "Nøtter", IsSystem = true, UsageCount = 42 };
        var writer = new RecordingWriter<IngredientCategory>();
        var handler = new InsertCatalogCommandHandler<IngredientCategory, Guid>(writer, Substitute.For<ICacheService>(), UnusedMediator());

        await handler.Handle(new InsertCatalogCommand<IngredientCategory, Guid>(entity), CancellationToken.None);

        Assert.False(writer.Inserted!.IsSystem);
        Assert.Equal(0, writer.Inserted.UsageCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_ReturnsInvalid_AndWritesNothing_WhenTheNameIsBlank(string name)
    {
        var entity = new IngredientCategory { Name = name };
        var writer = new RecordingWriter<IngredientCategory>();
        var cache = Substitute.For<ICacheService>();
        var handler = new InsertCatalogCommandHandler<IngredientCategory, Guid>(writer, cache, UnusedMediator());

        var result = await handler.Handle(new InsertCatalogCommand<IngredientCategory, Guid>(entity), CancellationToken.None);

        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Null(writer.Inserted);
        cache.DidNotReceive().Remove(Arg.Any<string>());
    }
}
