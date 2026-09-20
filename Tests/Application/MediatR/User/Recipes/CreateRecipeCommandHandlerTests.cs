using Application.MediatR.User.Recipes;
using Application.Results;
using Domain.Ingredients;
using Domain.Recipes;
using NSubstitute;
using Persistence.Interfaces;
using Tests.Application.MediatR.User.UnconfirmedIngredients;
using Xunit;

namespace Tests.Application.MediatR.User.Recipes;

public class CreateRecipeCommandHandlerTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly IRecipeReader _reader = Substitute.For<IRecipeReader>();
    private readonly IRecipeWriter _writer = Substitute.For<IRecipeWriter>();
    private readonly IUnconfirmedIngredientReader _unconfirmed = Substitute.For<IUnconfirmedIngredientReader>();

    private CreateRecipeCommandHandler Handler() => new(_reader, _writer, _unconfirmed, TimeProvider.System);

    [Fact]
    public async Task Handle_ReturnsInvalid_AndWritesNothing_ForAnInvalidRequest()
    {
        var result = await Handler().Handle(
            new CreateRecipeCommand(_userId, RecipeTestData.ValidRequest() with { Steps = [] }), CancellationToken.None);

        Assert.Equal(ResultStatus.Invalid, result.Status);
        await _writer.DidNotReceive().AddAsync(Arg.Any<Recipe>());
    }

    [Fact]
    public async Task Handle_ReturnsConflict_WhenTheUserHasReachedTheRecipeLimit()
    {
        _reader.CountByOwnerAsync(_userId).Returns(RecipeLimits.MaxPerUser);

        var result = await Handler().Handle(new CreateRecipeCommand(_userId, RecipeTestData.ValidRequest()), CancellationToken.None);

        Assert.Equal(ResultStatus.Conflict, result.Status);
        await _writer.DidNotReceive().AddAsync(Arg.Any<Recipe>());
    }

    [Fact]
    public async Task Handle_CreatesAManualRecipeOwnedByTheTokenUser_WithDerivedCookTimeAndNoFavorite()
    {
        Recipe? saved = null;
        await _writer.AddAsync(Arg.Do<Recipe>(r => saved = r));

        var result = await Handler().Handle(new CreateRecipeCommand(_userId, RecipeTestData.ValidRequest()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(saved);
        Assert.Equal(_userId, saved!.OwnerUserId);
        Assert.Equal(RecipeSourceType.Manual, saved.Source.Type);
        Assert.Null(saved.Source.Url);
        Assert.False(saved.IsFavorite);
        Assert.Equal(20, saved.CookTimeMinutes);
        Assert.Equal("kremet kyllinggryte", saved.Title);
        Assert.Equal(saved.CreatedAt, saved.UpdatedAt);
    }

    [Fact]
    public async Task Handle_ReturnsTheRecipeReadBackFromTheDatabase_SoIngredientNamesAreIncluded()
    {
        var readBack = RecipeTestData.ExistingRecipe(_userId);
        _reader.GetByIdAsync(Arg.Any<Guid>(), _userId).Returns(readBack);

        var result = await Handler().Handle(new CreateRecipeCommand(_userId, RecipeTestData.ValidRequest()), CancellationToken.None);

        Assert.Same(readBack, result.Value);
    }

    [Fact]
    public async Task Handle_RejectsAnUnconfirmedIngredientThatIsNotTheUsersOwn()
    {
        _unconfirmed.GetByUserAsync(_userId).Returns([]);
        var request = RecipeTestData.ValidRequest() with
        {
            Ingredients = [new RecipeIngredientRequest { UnconfirmedIngredientId = Guid.NewGuid(), UnitId = Guid.NewGuid() }]
        };

        var result = await Handler().Handle(new CreateRecipeCommand(_userId, request), CancellationToken.None);

        Assert.Equal(ResultStatus.Invalid, result.Status);
        await _writer.DidNotReceive().AddAsync(Arg.Any<Recipe>());
    }

    [Theory]
    [InlineData(UnconfirmedIngredientStatus.Approved)]
    [InlineData(UnconfirmedIngredientStatus.Merged)]
    public async Task Handle_RejectsAnUnconfirmedIngredientThatHasAlreadyBeenResolved(UnconfirmedIngredientStatus status)
    {
        var stub = UnconfirmedIngredientTestData.Stub(_userId, status);
        _unconfirmed.GetByUserAsync(_userId).Returns([stub]);
        var request = RecipeTestData.ValidRequest() with
        {
            Ingredients = [new RecipeIngredientRequest { UnconfirmedIngredientId = stub.Id, UnitId = Guid.NewGuid() }]
        };

        var result = await Handler().Handle(new CreateRecipeCommand(_userId, request), CancellationToken.None);

        Assert.Equal(ResultStatus.Invalid, result.Status);
    }

    [Theory]
    [InlineData(UnconfirmedIngredientStatus.NotRequested)]
    [InlineData(UnconfirmedIngredientStatus.Pending)]
    [InlineData(UnconfirmedIngredientStatus.Rejected)]
    public async Task Handle_AllowsTheUsersOwnUnresolvedUnconfirmedIngredient(UnconfirmedIngredientStatus status)
    {
        var stub = UnconfirmedIngredientTestData.Stub(_userId, status);
        _unconfirmed.GetByUserAsync(_userId).Returns([stub]);
        var request = RecipeTestData.ValidRequest() with
        {
            Ingredients = [new RecipeIngredientRequest { UnconfirmedIngredientId = stub.Id, UnitId = Guid.NewGuid() }]
        };

        var result = await Handler().Handle(new CreateRecipeCommand(_userId, request), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _writer.Received(1).AddAsync(Arg.Any<Recipe>());
    }
}
