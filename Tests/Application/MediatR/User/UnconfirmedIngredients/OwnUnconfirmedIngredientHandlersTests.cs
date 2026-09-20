using Application.MediatR.Catalog;
using Application.MediatR.User.UnconfirmedIngredients;
using Application.Results;
using MediatR;
using NSubstitute;
using Persistence.Interfaces;
using Domain.Ingredients;
using Xunit;

namespace Tests.Application.MediatR.User.UnconfirmedIngredients;

public class OwnUnconfirmedIngredientHandlersTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly IUnconfirmedIngredientReader _reader = Substitute.For<IUnconfirmedIngredientReader>();
    private readonly IUnconfirmedIngredientWriter _writer = Substitute.For<IUnconfirmedIngredientWriter>();
    private readonly IMediator _mediator = Substitute.For<IMediator>();

    public OwnUnconfirmedIngredientHandlersTests()
    {
        _mediator.Send(Arg.Any<GetAllCatalogQuery<IngredientListItem>>(), Arg.Any<CancellationToken>())
            .Returns(new List<IngredientListItem>());
    }

    [Fact]
    public async Task Get_ReturnsNotFound_ForAnotherUsersIngredient_ExactlyLikeAMissingOne()
    {
        var others = UnconfirmedIngredientTestData.Stub(Guid.NewGuid());
        _reader.GetByIdAsync(others.Id).Returns(others);
        var handler = new GetOwnUnconfirmedIngredientQueryHandler(_reader);

        var foreign = await handler.Handle(new GetOwnUnconfirmedIngredientQuery(_userId, others.Id), CancellationToken.None);
        var missing = await handler.Handle(new GetOwnUnconfirmedIngredientQuery(_userId, Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(ResultStatus.NotFound, foreign.Status);
        Assert.Equal(missing.Status, foreign.Status);
        Assert.Equal(missing.Error, foreign.Error);
    }

    [Fact]
    public async Task Get_ReturnsTheUsersOwnIngredient()
    {
        var own = UnconfirmedIngredientTestData.Stub(_userId);
        _reader.GetByIdAsync(own.Id).Returns(own);

        var result = await new GetOwnUnconfirmedIngredientQueryHandler(_reader)
            .Handle(new GetOwnUnconfirmedIngredientQuery(_userId, own.Id), CancellationToken.None);

        Assert.Same(own, result.Value);
    }

    [Fact]
    public async Task Rename_ChangesTheName_WhileNoRequestIsSent()
    {
        var own = UnconfirmedIngredientTestData.Stub(_userId);
        _reader.GetByIdAsync(own.Id).Returns(own);
        _writer.RenameAsync(own.Id, _userId, "ny").Returns(true);

        var result = await new RenameOwnUnconfirmedIngredientCommandHandler(_reader, _writer, _mediator)
            .Handle(new RenameOwnUnconfirmedIngredientCommand(_userId, own.Id, " Ny "), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("ny", result.Value!.Name);
    }

    [Fact]
    public async Task Rename_ReturnsConflict_WhenAnOfficialIngredientWithThatNameExists()
    {
        var own = UnconfirmedIngredientTestData.Stub(_userId);
        _reader.GetByIdAsync(own.Id).Returns(own);
        _mediator.Send(Arg.Any<GetAllCatalogQuery<IngredientListItem>>(), Arg.Any<CancellationToken>())
            .Returns(new List<IngredientListItem>
            {
                new()
                {
                    Id = Guid.NewGuid(), Name = "gulrot", CategoryId = Guid.NewGuid(), PrimaryUnitTypeId = Guid.NewGuid(),
                    DefaultUnitId = Guid.NewGuid(), EnergyKcal = 30, IsVerified = true, AllergenIds = [], SearchKeywordIds = []
                }
            });

        var result = await new RenameOwnUnconfirmedIngredientCommandHandler(_reader, _writer, _mediator)
            .Handle(new RenameOwnUnconfirmedIngredientCommand(_userId, own.Id, " Gulrot "), CancellationToken.None);

        Assert.Equal(ResultStatus.Conflict, result.Status);
        await _writer.DidNotReceiveWithAnyArgs().RenameAsync(default, default, default!);
    }

    [Theory]
    [InlineData(UnconfirmedIngredientStatus.Pending)]
    [InlineData(UnconfirmedIngredientStatus.Approved)]
    [InlineData(UnconfirmedIngredientStatus.Merged)]
    [InlineData(UnconfirmedIngredientStatus.Rejected)]
    public async Task Rename_ReturnsConflict_OnceARequestHasBeenSent(UnconfirmedIngredientStatus status)
    {
        var own = UnconfirmedIngredientTestData.Stub(_userId, status);
        _reader.GetByIdAsync(own.Id).Returns(own);

        var result = await new RenameOwnUnconfirmedIngredientCommandHandler(_reader, _writer, _mediator)
            .Handle(new RenameOwnUnconfirmedIngredientCommand(_userId, own.Id, "Ny"), CancellationToken.None);

        Assert.Equal(ResultStatus.Conflict, result.Status);
        await _writer.DidNotReceiveWithAnyArgs().RenameAsync(default, default, default!);
    }

    [Fact]
    public async Task Rename_ReturnsNotFound_ForAnotherUsersIngredient()
    {
        var others = UnconfirmedIngredientTestData.Stub(Guid.NewGuid());
        _reader.GetByIdAsync(others.Id).Returns(others);

        var result = await new RenameOwnUnconfirmedIngredientCommandHandler(_reader, _writer, _mediator)
            .Handle(new RenameOwnUnconfirmedIngredientCommand(_userId, others.Id, "Ny"), CancellationToken.None);

        Assert.Equal(ResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task RequestReview_MovesAPrivateIngredientToPending()
    {
        var own = UnconfirmedIngredientTestData.Stub(_userId);
        _reader.GetByIdAsync(own.Id).Returns(own);
        _writer.RequestReviewAsync(own.Id, _userId).Returns(true);

        var result = await new RequestReviewOwnUnconfirmedIngredientCommandHandler(_reader, _writer)
            .Handle(new RequestReviewOwnUnconfirmedIngredientCommand(_userId, own.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(UnconfirmedIngredientStatus.Pending, result.Value!.ReviewStatus);
    }

    [Fact]
    public async Task RequestReview_ReturnsConflict_WhenAlreadyRequested()
    {
        var own = UnconfirmedIngredientTestData.Stub(_userId, UnconfirmedIngredientStatus.Pending);
        _reader.GetByIdAsync(own.Id).Returns(own);

        var result = await new RequestReviewOwnUnconfirmedIngredientCommandHandler(_reader, _writer)
            .Handle(new RequestReviewOwnUnconfirmedIngredientCommand(_userId, own.Id), CancellationToken.None);

        Assert.Equal(ResultStatus.Conflict, result.Status);
    }

    [Fact]
    public async Task RequestReview_ReturnsConflict_WhenThePendingLimitIsReached()
    {
        var own = UnconfirmedIngredientTestData.Stub(_userId);
        _reader.GetByIdAsync(own.Id).Returns(own);
        _reader.CountByUserAsync(_userId, UnconfirmedIngredientStatus.Pending).Returns(UnconfirmedIngredientLimits.MaxPendingPerUser);

        var result = await new RequestReviewOwnUnconfirmedIngredientCommandHandler(_reader, _writer)
            .Handle(new RequestReviewOwnUnconfirmedIngredientCommand(_userId, own.Id), CancellationToken.None);

        Assert.Equal(ResultStatus.Conflict, result.Status);
        await _writer.DidNotReceiveWithAnyArgs().RequestReviewAsync(default, default);
    }

    [Theory]
    [InlineData(true, ResultStatus.Ok)]
    [InlineData(false, ResultStatus.NotFound)]
    public async Task Delete_MapsWhetherAnOwnRowWasDeleted(bool deleted, ResultStatus expected)
    {
        var id = Guid.NewGuid();
        _writer.DeleteOwnAsync(id, _userId).Returns(deleted);

        var result = await new DeleteOwnUnconfirmedIngredientCommandHandler(_writer)
            .Handle(new DeleteOwnUnconfirmedIngredientCommand(_userId, id), CancellationToken.None);

        Assert.Equal(expected, result.Status);
    }
}
