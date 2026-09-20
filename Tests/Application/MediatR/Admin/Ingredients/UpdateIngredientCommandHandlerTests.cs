using Application.Caching.Interfaces;
using Application.MediatR.Admin.Ingredients;
using Application.Results;
using Domain.Ingredients;
using NSubstitute;
using Xunit;

namespace Tests.Application.MediatR.Admin.Ingredients;

public class UpdateIngredientCommandHandlerTests
{
    private static Ingredient Existing(Guid id) => IngredientMapper.ToIngredient(IngredientTestData.ValidRequest(), id);

    [Fact]
    public async Task Handle_ReturnsNotFound_AndWritesNothing_WhenTheIngredientDoesNotExist()
    {
        var writer = new FakeIngredientWriter();
        var handler = new UpdateIngredientCommandHandler(
            new FakeIngredientReader(null), writer, Substitute.For<ICacheService>());

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
        var handler = new UpdateIngredientCommandHandler(new FakeIngredientReader(Existing(id)), writer, cache);

        var result = await handler.Handle(
            new UpdateIngredientCommand(id, IngredientTestData.ValidRequest() with { Name = "Slangeagurk" }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(id, writer.Updated!.Id);
        Assert.Equal("Slangeagurk", writer.Updated.Name);
        cache.Received(1).Remove(IngredientTestData.IngredientListCacheKey);
    }
}
