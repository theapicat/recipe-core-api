using Application.Caching.Interfaces;
using Application.MediatR.Admin.Ingredients;
using Application.Results;
using MediatR;
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
        var handler = new CreateIngredientCommandHandler(writer, cache, IngredientTestData.PassthroughMediator(), TimeProvider.System);

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
        var handler = new CreateIngredientCommandHandler(writer, cache, IngredientTestData.PassthroughMediator(), TimeProvider.System);

        var result = await handler.Handle(new CreateIngredientCommand(IngredientTestData.ValidRequest()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value!.Id);
        Assert.False(result.Value.IsOfficial);
        Assert.Same(result.Value, writer.Added);
        cache.Received(1).Remove(IngredientTestData.IngredientListCacheKey);
    }

    [Fact]
    public async Task Handle_ReturnsInvalid_AndWritesNothing_WhenForeignKeyValidationFails()
    {
        var writer = new FakeIngredientWriter();
        var cache = Substitute.For<ICacheService>();
        var handler = new CreateIngredientCommandHandler(writer, cache, IngredientTestData.PassthroughMediator(), TimeProvider.System);
        var request = IngredientTestData.ValidRequest() with { CategoryId = Guid.NewGuid() };

        var result = await handler.Handle(new CreateIngredientCommand(request), CancellationToken.None);

        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Null(writer.Added);
        cache.DidNotReceive().Remove(Arg.Any<string>());
    }
}
