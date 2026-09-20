using Application.Caching.Interfaces;
using Application.MediatR.Admin.Ingredients;
using Application.Results;
using NSubstitute;
using Xunit;

namespace Tests.Application.MediatR.Admin.Ingredients;

public class DeleteIngredientCommandHandlerTests
{
    [Theory]
    [InlineData(1, ResultStatus.Ok)]
    [InlineData(0, ResultStatus.NotFound)]
    public async Task Handle_MapsDeletedRowCountToAResult_AndInvalidatesTheListCache(int deletedRows, ResultStatus expected)
    {
        var cache = Substitute.For<ICacheService>();
        var handler = new DeleteIngredientCommandHandler(new FakeIngredientWriter(deletedRows), cache);

        var result = await handler.Handle(new DeleteIngredientCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(expected, result.Status);
        cache.Received(1).Remove(IngredientTestData.IngredientListCacheKey);
    }
}
