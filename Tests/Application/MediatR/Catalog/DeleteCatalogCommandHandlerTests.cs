using Application.Caching.Interfaces;
using Application.MediatR.Catalog;
using Application.Results;
using Domain.Ingredients;
using NSubstitute;
using Persistence.Services;
using Xunit;

namespace Tests.Application.MediatR.Catalog;

public class DeleteCatalogCommandHandlerTests
{
    private class FakeReader(IngredientCategory? existing) : DbReader<IngredientCategory>("unused")
    {
        public override Task<IngredientCategory?> GetByIdAsync<TId>(TId id) => Task.FromResult(existing);
    }

    private class FakeWriter() : DbWriter<IngredientCategory>("unused")
    {
        public Guid? Deleted { get; private set; }
        public override Task<int> DeleteAsync<TId>(TId id)
        {
            Deleted = (Guid)(object)id!;
            return Task.FromResult(1);
        }
    }

    private static IngredientCategory Row(bool isSystem = false, int usageCount = 0) =>
        new() { Id = Guid.NewGuid(), Name = "frukt", IsSystem = isSystem, UsageCount = usageCount };

    [Fact]
    public async Task Handle_ReturnsNotFound_AndWritesNothing_WhenTheRowDoesNotExist()
    {
        var writer = new FakeWriter();
        var cache = Substitute.For<ICacheService>();
        var handler = new DeleteCatalogCommandHandler<IngredientCategory, Guid>(new FakeReader(null), writer, cache);

        var result = await handler.Handle(new DeleteCatalogCommand<IngredientCategory, Guid>(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(ResultStatus.NotFound, result.Status);
        Assert.Null(writer.Deleted);
        cache.DidNotReceive().Remove(Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_ReturnsConflict_AndWritesNothing_ForASystemRow()
    {
        var writer = new FakeWriter();
        var cache = Substitute.For<ICacheService>();
        var handler = new DeleteCatalogCommandHandler<IngredientCategory, Guid>(new FakeReader(Row(isSystem: true)), writer, cache);

        var result = await handler.Handle(new DeleteCatalogCommand<IngredientCategory, Guid>(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(ResultStatus.Conflict, result.Status);
        Assert.Null(writer.Deleted);
        cache.DidNotReceive().Remove(Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_ReturnsConflict_AndWritesNothing_ForARowStillInUse()
    {
        var writer = new FakeWriter();
        var cache = Substitute.For<ICacheService>();
        var handler = new DeleteCatalogCommandHandler<IngredientCategory, Guid>(new FakeReader(Row(usageCount: 3)), writer, cache);

        var result = await handler.Handle(new DeleteCatalogCommand<IngredientCategory, Guid>(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(ResultStatus.Conflict, result.Status);
        Assert.Contains("3", result.Error);
        Assert.Null(writer.Deleted);
        cache.DidNotReceive().Remove(Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_DeletesTheRow_AndInvalidatesTheListCache_WhenNotSystemOrInUse()
    {
        var id = Guid.NewGuid();
        var writer = new FakeWriter();
        var cache = Substitute.For<ICacheService>();
        var handler = new DeleteCatalogCommandHandler<IngredientCategory, Guid>(new FakeReader(Row()), writer, cache);

        var result = await handler.Handle(new DeleteCatalogCommand<IngredientCategory, Guid>(id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(id, writer.Deleted);
        cache.Received(1).Remove(Arg.Any<string>());
    }
}
