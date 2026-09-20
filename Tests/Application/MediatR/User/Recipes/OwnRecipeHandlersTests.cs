using Application.MediatR.User.Recipes;
using Application.Results;
using Domain.Recipes;
using NSubstitute;
using Persistence.Interfaces;
using Xunit;

namespace Tests.Application.MediatR.User.Recipes;

public class OwnRecipeHandlersTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly IRecipeReader _reader = Substitute.For<IRecipeReader>();
    private readonly IRecipeWriter _writer = Substitute.For<IRecipeWriter>();

    [Fact]
    public async Task GetAll_ReturnsTheOwnersListFromTheReader()
    {
        var list = new List<RecipeListItem>
        {
            new() { Id = Guid.NewGuid(), Title = "pannekaker", CategoryId = Guid.NewGuid(), CookTimeMinutes = 10, Servings = 4, IsFavorite = false }
        };
        _reader.GetListByOwnerAsync(_userId).Returns(list);

        var result = await new GetOwnRecipesQueryHandler(_reader).Handle(new GetOwnRecipesQuery(_userId), CancellationToken.None);

        Assert.Same(list, result);
    }

    [Fact]
    public async Task Get_ReturnsTheRecipe_WhenItIsTheUsers()
    {
        var recipe = RecipeTestData.ExistingRecipe(_userId);
        _reader.GetByIdAsync(recipe.Id, _userId).Returns(recipe);

        var result = await new GetOwnRecipeQueryHandler(_reader).Handle(new GetOwnRecipeQuery(_userId, recipe.Id), CancellationToken.None);

        Assert.Same(recipe, result.Value);
    }

    // Eier sendes alltid med til readeren, som filtrerer på den - en annen brukers oppskrift er identisk med en som ikke finnes.
    [Fact]
    public async Task Get_ReturnsNotFound_WhenTheReaderFindsNothing()
    {
        _reader.GetByIdAsync(Arg.Any<Guid>(), _userId).Returns((Recipe?)null);

        var result = await new GetOwnRecipeQueryHandler(_reader).Handle(new GetOwnRecipeQuery(_userId, Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(ResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task Delete_Succeeds_WhenARowWasDeleted()
    {
        var id = Guid.NewGuid();
        _writer.DeleteAsync(id, _userId).Returns(true);

        var result = await new DeleteRecipeCommandHandler(_writer).Handle(new DeleteRecipeCommand(_userId, id), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenNothingMatched()
    {
        _writer.DeleteAsync(Arg.Any<Guid>(), _userId).Returns(false);

        var result = await new DeleteRecipeCommandHandler(_writer).Handle(new DeleteRecipeCommand(_userId, Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(ResultStatus.NotFound, result.Status);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SetFavorite_PassesTheOwnerAndTheValueToTheWriter(bool isFavorite)
    {
        var id = Guid.NewGuid();
        _writer.SetFavoriteAsync(id, _userId, isFavorite).Returns(true);

        var result = await new SetRecipeFavoriteCommandHandler(_writer)
            .Handle(new SetRecipeFavoriteCommand(_userId, id, isFavorite), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _writer.Received(1).SetFavoriteAsync(id, _userId, isFavorite);
    }

    [Fact]
    public async Task SetFavorite_ReturnsNotFound_WhenNothingMatched()
    {
        _writer.SetFavoriteAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>()).Returns(false);

        var result = await new SetRecipeFavoriteCommandHandler(_writer)
            .Handle(new SetRecipeFavoriteCommand(_userId, Guid.NewGuid(), true), CancellationToken.None);

        Assert.Equal(ResultStatus.NotFound, result.Status);
    }
}
