using Application.MediatR.User.UnconfirmedIngredients;
using Application.MediatR.Catalog;
using Application.Results;
using Domain.Ingredients;
using MediatR;
using NSubstitute;
using Persistence.Interfaces;
using Xunit;

namespace Tests.Application.MediatR.User.UnconfirmedIngredients;

public class CreateUnconfirmedIngredientCommandHandlerTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly IUnconfirmedIngredientReader _reader = Substitute.For<IUnconfirmedIngredientReader>();
    private readonly IUnconfirmedIngredientWriter _writer = Substitute.For<IUnconfirmedIngredientWriter>();

    private readonly IMediator _mediator = Substitute.For<IMediator>();

    public CreateUnconfirmedIngredientCommandHandlerTests()
    {
        _mediator.Send(Arg.Any<GetAllCatalogQuery<IngredientListItem>>(), Arg.Any<CancellationToken>())
            .Returns(new List<IngredientListItem>());
    }

    private CreateUnconfirmedIngredientCommandHandler Handler() => new(_reader, _writer, _mediator, TimeProvider.System);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_RejectsABlankName(string name)
    {
        var result = await Handler().Handle(new CreateUnconfirmedIngredientCommand(_userId, name, false), CancellationToken.None);

        Assert.Equal(ResultStatus.Invalid, result.Status);
        await _writer.DidNotReceive().AddAsync(Arg.Any<UnconfirmedIngredient>());
    }

    [Fact]
    public async Task Handle_RejectsANameThatIsTooLong()
    {
        var name = new string('x', UnconfirmedIngredientLimits.MaxNameLength + 1);

        var result = await Handler().Handle(new CreateUnconfirmedIngredientCommand(_userId, name, false), CancellationToken.None);

        Assert.Equal(ResultStatus.Invalid, result.Status);
    }

    [Fact]
    public async Task Handle_ReturnsConflict_WhenAnOfficialIngredientWithTheSameNameExists()
    {
        _mediator.Send(Arg.Any<GetAllCatalogQuery<IngredientListItem>>(), Arg.Any<CancellationToken>())
            .Returns(new List<IngredientListItem> { OfficialItem("gulrot") });

        var result = await Handler().Handle(new CreateUnconfirmedIngredientCommand(_userId, "  Gulrot ", false), CancellationToken.None);

        Assert.Equal(ResultStatus.Conflict, result.Status);
        await _writer.DidNotReceive().AddAsync(Arg.Any<UnconfirmedIngredient>());
    }

    private static IngredientListItem OfficialItem(string name) => new()
    {
        Id = Guid.NewGuid(), Name = name, CategoryId = Guid.NewGuid(), PrimaryUnitTypeId = Guid.NewGuid(),
        DefaultUnitId = Guid.NewGuid(), EnergyKcal = 30, IsVerified = true, IsOfficial = true, AllergenIds = [], SearchKeywordIds = []
    };

    [Fact]
    public async Task Handle_ReturnsConflict_WhenTheUserHasReachedTheTotalLimit()
    {
        _reader.CountByUserAsync(_userId, null).Returns(UnconfirmedIngredientLimits.MaxPerUser);

        var result = await Handler().Handle(new CreateUnconfirmedIngredientCommand(_userId, "Gulrot", false), CancellationToken.None);

        Assert.Equal(ResultStatus.Conflict, result.Status);
        await _writer.DidNotReceive().AddAsync(Arg.Any<UnconfirmedIngredient>());
    }

    [Fact]
    public async Task Handle_ReturnsConflict_WhenTheUserHasTooManyPendingRequests_AndRequestsAnother()
    {
        _reader.CountByUserAsync(_userId, UnconfirmedIngredientStatus.Pending).Returns(UnconfirmedIngredientLimits.MaxPendingPerUser);

        var result = await Handler().Handle(new CreateUnconfirmedIngredientCommand(_userId, "Gulrot", true), CancellationToken.None);

        Assert.Equal(ResultStatus.Conflict, result.Status);
    }

    [Fact]
    public async Task Handle_AllowsAPrivateIngredient_EvenWhenThePendingLimitIsReached()
    {
        _reader.CountByUserAsync(_userId, UnconfirmedIngredientStatus.Pending).Returns(UnconfirmedIngredientLimits.MaxPendingPerUser);

        var result = await Handler().Handle(new CreateUnconfirmedIngredientCommand(_userId, "Gulrot", false), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData(false, UnconfirmedIngredientStatus.NotRequested)]
    [InlineData(true, UnconfirmedIngredientStatus.Pending)]
    public async Task Handle_CreatesTheStubForTheTokenUser_WithTheRightInitialStatus(bool requestReview, UnconfirmedIngredientStatus expected)
    {
        var result = await Handler().Handle(
            new CreateUnconfirmedIngredientCommand(_userId, "  Lilla gulrot  ", requestReview), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(_userId, result.Value!.CreatedByUserId);
        Assert.Equal("lilla gulrot", result.Value.Name);
        Assert.Equal(expected, result.Value.ReviewStatus);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
        await _writer.Received(1).AddAsync(result.Value);
    }
}
