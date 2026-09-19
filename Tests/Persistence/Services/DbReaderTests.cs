using Domain.Ingredients;
using Persistence.Exceptions;
using Persistence.Services;
using Xunit;

namespace Tests.Persistence.Services;

public class DbReaderTests
{
    private class UnconfiguredReader() : DbReader<IngredientCategory>("unused");

    [Fact]
    public async Task GetAllAsync_Throws_WhenGetAllQueryNotConfigured()
    {
        var reader = new UnconfiguredReader();

        await Assert.ThrowsAsync<DbQueryMissingException<IngredientCategory>>(() => reader.GetAllAsync());
    }

    [Fact]
    public async Task GetByIdAsync_Throws_WhenGetByIdQueryNotConfigured()
    {
        var reader = new UnconfiguredReader();

        await Assert.ThrowsAsync<DbQueryMissingException<IngredientCategory>>(() => reader.GetByIdAsync(Guid.NewGuid()));
    }
}
