using Application.Caching.Interfaces;
using Application.MediatR.Admin.Ingredients;
using Application.Results;
using Domain.Ingredients;
using NSubstitute;
using Xunit;

namespace Tests.Application.MediatR.Admin.Ingredients;

public class DeleteIngredientCommandHandlerTests
{
    private static Ingredient Existing(int usageCount = 0)
    {
        var ingredient = IngredientMapper.ToIngredient(
            IngredientTestData.ValidRequest(), Guid.NewGuid(), isOfficial: false, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        ingredient.UsageCount = usageCount;
        return ingredient;
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenTheIngredientDoesNotExist()
    {
        var cache = Substitute.For<ICacheService>();
        var handler = new DeleteIngredientCommandHandler(new FakeIngredientReader(null), new FakeIngredientWriter(), cache);

        var result = await handler.Handle(new DeleteIngredientCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(ResultStatus.NotFound, result.Status);
        cache.DidNotReceive().Remove(Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_ReturnsConflict_WhenTheIngredientIsStillInUse()
    {
        var cache = Substitute.For<ICacheService>();
        var handler = new DeleteIngredientCommandHandler(new FakeIngredientReader(Existing(usageCount: 2)), new FakeIngredientWriter(), cache);

        var result = await handler.Handle(new DeleteIngredientCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(ResultStatus.Conflict, result.Status);
        Assert.Contains("2", result.Error);
        cache.DidNotReceive().Remove(Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_DeletesTheIngredient_AndInvalidatesTheListCache_WhenNotInUse()
    {
        var cache = Substitute.For<ICacheService>();
        var handler = new DeleteIngredientCommandHandler(new FakeIngredientReader(Existing()), new FakeIngredientWriter(), cache);

        var result = await handler.Handle(new DeleteIngredientCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        cache.Received(1).Remove(IngredientTestData.IngredientListCacheKey);
    }
}
