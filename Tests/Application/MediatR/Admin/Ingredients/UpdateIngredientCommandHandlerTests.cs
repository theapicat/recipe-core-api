using Application.Caching.Interfaces;
using Application.MediatR.Admin.Ingredients;
using Application.Results;
using Domain.Ingredients;
using MediatR;
using NSubstitute;
using Xunit;

namespace Tests.Application.MediatR.Admin.Ingredients;

public class UpdateIngredientCommandHandlerTests
{
    private static Ingredient Existing(Guid id, bool isOfficial = false) =>
        IngredientMapper.ToIngredient(IngredientTestData.ValidRequest(), id, isOfficial, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

    [Fact]
    public async Task Handle_ReturnsNotFound_AndWritesNothing_WhenTheIngredientDoesNotExist()
    {
        var writer = new FakeIngredientWriter();
        var handler = new UpdateIngredientCommandHandler(
            new FakeIngredientReader(null), writer, Substitute.For<ICacheService>(), IngredientTestData.PassthroughMediator(), TimeProvider.System);

        var result = await handler.Handle(
            new UpdateIngredientCommand(Guid.NewGuid(), IngredientTestData.ValidRequest()), CancellationToken.None);

        Assert.Equal(ResultStatus.NotFound, result.Status);
        Assert.Null(writer.Updated);
    }

    [Fact]
    public async Task Handle_UpdatesUnderTheRouteId_AndInvalidatesTheListCache()
    {
        var id = Guid.NewGuid();
        var writer = new FakeIngredientWriter();
        var cache = Substitute.For<ICacheService>();
        var handler = new UpdateIngredientCommandHandler(
            new FakeIngredientReader(Existing(id)), writer, cache, IngredientTestData.PassthroughMediator(), TimeProvider.System);

        var result = await handler.Handle(
            new UpdateIngredientCommand(id, IngredientTestData.ValidRequest() with { Name = "Slangeagurk" }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(id, writer.Updated!.Id);
        Assert.Equal("slangeagurk", writer.Updated.Name);
        cache.Received(1).Remove(IngredientTestData.IngredientListCacheKey);
    }

    [Fact]
    public async Task Handle_PreservesIsOfficial_RegardlessOfTheRequest()
    {
        var id = Guid.NewGuid();
        var writer = new FakeIngredientWriter();
        var existing = Existing(id, isOfficial: true);
        // Uendret kildedata - den offisielle låsen blokkerer ikke, IsOfficial finnes ikke som felt på requesten uansett.
        var handler = new UpdateIngredientCommandHandler(
            new FakeIngredientReader(existing), writer, Substitute.For<ICacheService>(), IngredientTestData.PassthroughMediator(), TimeProvider.System);

        var result = await handler.Handle(new UpdateIngredientCommand(id, IngredientTestData.ValidRequest()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(writer.Updated!.IsOfficial);
    }

    [Fact]
    public async Task Handle_PreservesCreatedAt_RegardlessOfTheRequest()
    {
        var id = Guid.NewGuid();
        var existing = Existing(id);
        var writer = new FakeIngredientWriter();
        var handler = new UpdateIngredientCommandHandler(
            new FakeIngredientReader(existing), writer, Substitute.For<ICacheService>(), IngredientTestData.PassthroughMediator(), TimeProvider.System);

        var result = await handler.Handle(new UpdateIngredientCommand(id, IngredientTestData.ValidRequest()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(existing.CreatedAt, writer.Updated!.CreatedAt);
    }

    [Fact]
    public async Task Handle_SetsUpdatedAtToNow()
    {
        var id = Guid.NewGuid();
        var writer = new FakeIngredientWriter();
        var handler = new UpdateIngredientCommandHandler(
            new FakeIngredientReader(Existing(id)), writer, Substitute.For<ICacheService>(), IngredientTestData.PassthroughMediator(), TimeProvider.System);
        var before = DateTimeOffset.UtcNow;

        await handler.Handle(new UpdateIngredientCommand(id, IngredientTestData.ValidRequest()), CancellationToken.None);

        Assert.True(writer.Updated!.UpdatedAt >= before);
    }

    [Fact]
    public async Task Handle_ReturnsConflict_WhenTheClientsUpdatedAtIsOlderThanStored()
    {
        var id = Guid.NewGuid();
        var existing = Existing(id);
        var writer = new FakeIngredientWriter();
        var handler = new UpdateIngredientCommandHandler(
            new FakeIngredientReader(existing), writer, Substitute.For<ICacheService>(), IngredientTestData.PassthroughMediator(), TimeProvider.System);
        var staleRequest = IngredientTestData.ValidRequest() with { UpdatedAt = existing.UpdatedAt.AddMinutes(-1) };

        var result = await handler.Handle(new UpdateIngredientCommand(id, staleRequest), CancellationToken.None);

        Assert.Equal(ResultStatus.Conflict, result.Status);
        Assert.Null(writer.Updated);
    }

    [Fact]
    public async Task Handle_Succeeds_WhenTheClientsUpdatedAtMatchesOrIsNewer()
    {
        var id = Guid.NewGuid();
        var existing = Existing(id);
        var writer = new FakeIngredientWriter();
        var handler = new UpdateIngredientCommandHandler(
            new FakeIngredientReader(existing), writer, Substitute.For<ICacheService>(), IngredientTestData.PassthroughMediator(), TimeProvider.System);
        var currentRequest = IngredientTestData.ValidRequest() with { UpdatedAt = existing.UpdatedAt };

        var result = await handler.Handle(new UpdateIngredientCommand(id, currentRequest), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ReturnsInvalid_WhenAnOfficialIngredientsSourceDataChanges()
    {
        var id = Guid.NewGuid();
        var existing = Existing(id, isOfficial: true);
        var writer = new FakeIngredientWriter();
        var handler = new UpdateIngredientCommandHandler(
            new FakeIngredientReader(existing), writer, Substitute.For<ICacheService>(), IngredientTestData.PassthroughMediator(), TimeProvider.System);
        var request = IngredientTestData.ValidRequest() with { EnergyKcal = existing.EnergyKcal + 1 };

        var result = await handler.Handle(new UpdateIngredientCommand(id, request), CancellationToken.None);

        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Null(writer.Updated);
    }

    [Fact]
    public async Task Handle_ReturnsInvalid_WhenForeignKeyValidationFails()
    {
        var id = Guid.NewGuid();
        var writer = new FakeIngredientWriter();
        var handler = new UpdateIngredientCommandHandler(
            new FakeIngredientReader(Existing(id)), writer, Substitute.For<ICacheService>(), IngredientTestData.PassthroughMediator(), TimeProvider.System);
        var request = IngredientTestData.ValidRequest() with { CategoryId = Guid.NewGuid() };

        var result = await handler.Handle(new UpdateIngredientCommand(id, request), CancellationToken.None);

        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Null(writer.Updated);
    }
}
