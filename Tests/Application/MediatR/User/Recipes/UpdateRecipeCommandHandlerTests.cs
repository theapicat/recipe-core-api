using Application.MediatR.User.Recipes;
using Application.Results;
using Domain.Recipes;
using NSubstitute;
using Persistence.Interfaces;
using Xunit;

namespace Tests.Application.MediatR.User.Recipes;

public class UpdateRecipeCommandHandlerTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly IRecipeReader _reader = Substitute.For<IRecipeReader>();
    private readonly IRecipeWriter _writer = Substitute.For<IRecipeWriter>();
    private readonly IUnconfirmedIngredientReader _unconfirmed = Substitute.For<IUnconfirmedIngredientReader>();

    private UpdateRecipeCommandHandler Handler() => new(_reader, _writer, _unconfirmed, TimeProvider.System);

    [Fact]
    public async Task Handle_ReturnsInvalid_ForAnInvalidRequest()
    {
        var result = await Handler().Handle(
            new UpdateRecipeCommand(_userId, Guid.NewGuid(), RecipeTestData.ValidRequest() with { Title = " " }), CancellationToken.None);

        Assert.Equal(ResultStatus.Invalid, result.Status);
        await _writer.DidNotReceive().UpdateAsync(Arg.Any<Recipe>());
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenTheRecipeIsMissingOrNotTheUsers()
    {
        _reader.GetByIdAsync(Arg.Any<Guid>(), _userId).Returns((Recipe?)null);

        var result = await Handler().Handle(
            new UpdateRecipeCommand(_userId, Guid.NewGuid(), RecipeTestData.ValidRequest()), CancellationToken.None);

        Assert.Equal(ResultStatus.NotFound, result.Status);
        await _writer.DidNotReceive().UpdateAsync(Arg.Any<Recipe>());
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenTheWriterMatchesNoRow()
    {
        var existing = RecipeTestData.ExistingRecipe(_userId);
        _reader.GetByIdAsync(existing.Id, _userId).Returns(existing);
        _writer.UpdateAsync(Arg.Any<Recipe>()).Returns(false);

        var result = await Handler().Handle(
            new UpdateRecipeCommand(_userId, existing.Id, RecipeTestData.ValidRequest()), CancellationToken.None);

        Assert.Equal(ResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task Handle_KeepsIdOwnerFavoriteAndCreatedAt_AndChangesTheRest()
    {
        var existing = RecipeTestData.ExistingRecipe(_userId);
        _reader.GetByIdAsync(existing.Id, _userId).Returns(existing);
        Recipe? saved = null;
        _writer.UpdateAsync(Arg.Do<Recipe>(r => saved = r)).Returns(true);

        var result = await Handler().Handle(
            new UpdateRecipeCommand(_userId, existing.Id, RecipeTestData.ValidRequest()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(existing.Id, saved!.Id);
        Assert.Equal(_userId, saved.OwnerUserId);
        Assert.True(saved.IsFavorite);
        Assert.Equal(existing.CreatedAt, saved.CreatedAt);
        Assert.True(saved.UpdatedAt > existing.UpdatedAt);
        Assert.Equal("kremet kyllinggryte", saved.Title);
        Assert.Equal(20, saved.CookTimeMinutes);
    }

    [Fact]
    public async Task Handle_MarksAScrapedRecipeAsEditedFromSource_AndKeepsItsLockedTypeAndUrl()
    {
        var existing = RecipeTestData.ExistingRecipe(_userId, RecipeSourceType.Scraped);
        _reader.GetByIdAsync(existing.Id, _userId).Returns(existing);
        Recipe? saved = null;
        _writer.UpdateAsync(Arg.Do<Recipe>(r => saved = r)).Returns(true);

        await Handler().Handle(
            new UpdateRecipeCommand(_userId, existing.Id, RecipeTestData.ValidRequest() with
            {
                Source = new RecipeSourceRequest { Reference = "min egen note" }
            }), CancellationToken.None);

        Assert.Equal(RecipeSourceType.Scraped, saved!.Source.Type);
        Assert.Equal("https://example.test/oppskrift", saved.Source.Url);
        Assert.True(saved.Source.IsEditedFromSource);
        Assert.Equal("min egen note", saved.Source.Reference);
    }

    [Fact]
    public async Task Handle_LeavesTheEditedFlagAloneForAManualRecipe()
    {
        var existing = RecipeTestData.ExistingRecipe(_userId, RecipeSourceType.Manual);
        _reader.GetByIdAsync(existing.Id, _userId).Returns(existing);
        Recipe? saved = null;
        _writer.UpdateAsync(Arg.Do<Recipe>(r => saved = r)).Returns(true);

        await Handler().Handle(new UpdateRecipeCommand(_userId, existing.Id, RecipeTestData.ValidRequest()), CancellationToken.None);

        Assert.Equal(RecipeSourceType.Manual, saved!.Source.Type);
        Assert.Null(saved.Source.IsEditedFromSource);
    }
}
