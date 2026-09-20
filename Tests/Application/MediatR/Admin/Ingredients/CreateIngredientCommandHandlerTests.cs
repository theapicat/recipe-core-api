using Application.Caching.Interfaces;
using Application.MediatR.Admin.Ingredients;
using Application.Results;
using NSubstitute;
using Xunit;

namespace Tests.Application.MediatR.Admin.Ingredients;

public class CreateIngredientCommandHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsInvalid_AndWritesNothing_ForAnInvalidRequest()
    {
        var writer = new FakeIngredientWriter();
        var cache = Substitute.For<ICacheService>();
        var handler = new CreateIngredientCommandHandler(writer, cache);

        var result = await handler.Handle(
            new CreateIngredientCommand(IngredientTestData.ValidRequest() with { Name = "" }), CancellationToken.None);

        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Null(writer.Added);
        cache.DidNotReceive().Remove(Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_WritesTheIngredientWithAServerAssignedId_AndInvalidatesTheListCache()
    {
        var writer = new FakeIngredientWriter();
        var cache = Substitute.For<ICacheService>();
        var handler = new CreateIngredientCommandHandler(writer, cache);

        var result = await handler.Handle(new CreateIngredientCommand(IngredientTestData.ValidRequest()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value!.Id);
        Assert.Same(result.Value, writer.Added);
        cache.Received(1).Remove(IngredientTestData.IngredientListCacheKey);
    }
}
