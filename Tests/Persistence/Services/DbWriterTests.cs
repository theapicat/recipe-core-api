using Domain.Ingredients;
using Persistence.Exceptions;
using Persistence.Services;
using Xunit;

namespace Tests.Persistence.Services;

public class DbWriterTests
{
    private class UnconfiguredWriter() : DbWriter<IngredientCategory>("unused");

    [Fact]
    public async Task AddAsync_Throws_WhenInsertCommandNotConfigured()
    {
        var writer = new UnconfiguredWriter();
        var entity = new IngredientCategory { Id = Guid.NewGuid(), Name = "Test" };

        await Assert.ThrowsAsync<DbCommandMissingException<IngredientCategory>>(() => writer.AddAsync(entity));
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenUpdateCommandNotConfigured()
    {
        var writer = new UnconfiguredWriter();
        var entity = new IngredientCategory { Id = Guid.NewGuid(), Name = "Test" };

        await Assert.ThrowsAsync<DbCommandMissingException<IngredientCategory>>(() => writer.UpdateAsync(entity));
    }

    [Fact]
    public async Task DeleteAsync_Throws_WhenDeleteCommandNotConfigured()
    {
        var writer = new UnconfiguredWriter();

        await Assert.ThrowsAsync<DbCommandMissingException<IngredientCategory>>(() => writer.DeleteAsync(Guid.NewGuid()));
    }
}
