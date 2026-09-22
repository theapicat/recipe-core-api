using Application.Caching.Interfaces;
using Application.MediatR.Admin.Ingredients;
using Application.MediatR.Admin.UnconfirmedIngredients;
using Application.MediatR.Catalog;
using Application.Results;
using Domain.Ingredients;
using MediatR;
using NSubstitute;
using Persistence.Interfaces;
using Tests.Application.MediatR.Admin.Ingredients;
using Tests.Application.MediatR.User.UnconfirmedIngredients;
using Xunit;

namespace Tests.Application.MediatR.Admin.UnconfirmedIngredients;

public class AdminUnconfirmedIngredientHandlersTests
{
    private readonly IUnconfirmedIngredientReader _reader = Substitute.For<IUnconfirmedIngredientReader>();
    private readonly IUnconfirmedIngredientWriter _writer = Substitute.For<IUnconfirmedIngredientWriter>();
    private readonly ICacheService _cache = Substitute.For<ICacheService>();
    private readonly IMediator _mediator = IngredientTestData.PassthroughMediator();

    private UnconfirmedIngredient PendingStub()
    {
        var stub = UnconfirmedIngredientTestData.Stub(Guid.NewGuid(), UnconfirmedIngredientStatus.Pending);
        _reader.GetByIdAsync(stub.Id).Returns(stub);
        return stub;
    }

    private ApproveUnconfirmedIngredientCommandHandler ApproveHandler() => new(_reader, _writer, _cache, _mediator, TimeProvider.System);

    [Fact]
    public async Task Approve_ReturnsNotFound_ForAnUnknownRequest()
    {
        var result = await ApproveHandler().Handle(
            new ApproveUnconfirmedIngredientCommand(Guid.NewGuid(), IngredientTestData.ValidRequest()), CancellationToken.None);

        Assert.Equal(ResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task Approve_ReturnsInvalid_ForAnInvalidIngredient()
    {
        var stub = PendingStub();

        var result = await ApproveHandler().Handle(
            new ApproveUnconfirmedIngredientCommand(stub.Id, IngredientTestData.ValidRequest() with { Name = "" }), CancellationToken.None);

        Assert.Equal(ResultStatus.Invalid, result.Status);
    }

    [Fact]
    public async Task Approve_ReturnsConflict_WhenTheRequestIsNoLongerPending()
    {
        var stub = UnconfirmedIngredientTestData.Stub(Guid.NewGuid(), UnconfirmedIngredientStatus.Rejected);
        _reader.GetByIdAsync(stub.Id).Returns(stub);

        var result = await ApproveHandler().Handle(
            new ApproveUnconfirmedIngredientCommand(stub.Id, IngredientTestData.ValidRequest()), CancellationToken.None);

        Assert.Equal(ResultStatus.Conflict, result.Status);
    }

    [Fact]
    public async Task Approve_CreatesTheIngredientAndResolvesTheRequestAsApproved_ThenInvalidatesTheListCache()
    {
        var stub = PendingStub();
        _writer.ResolveAsync(default, default, default, default, default).ReturnsForAnyArgs(true);
        var baseId = Guid.NewGuid();
        _mediator.Send(Arg.Is<GetCatalogByIdQuery<Ingredient, Guid>>(q => q.Id == baseId), Arg.Any<CancellationToken>())
            .Returns(IngredientMapper.ToIngredient(IngredientTestData.ValidRequest(), baseId, isOfficial: false, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow));

        var result = await ApproveHandler().Handle(
            new ApproveUnconfirmedIngredientCommand(stub.Id, IngredientTestData.ValidRequest() with { VariantOfIngredientId = baseId }),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(baseId, result.Value!.VariantOfIngredientId);
        await _writer.Received(1).ResolveAsync(
            stub.Id, UnconfirmedIngredientStatus.Approved, result.Value.Id, result.Value, Arg.Any<DateTimeOffset>());
        _cache.Received(1).Remove(IngredientTestData.IngredientListCacheKey);
    }

    [Fact]
    public async Task Approve_ReturnsConflict_AndKeepsTheCache_WhenTheWriterReportsItWasNotPending()
    {
        var stub = PendingStub();
        _writer.ResolveAsync(default, default, default, default, default).ReturnsForAnyArgs(false);

        var result = await ApproveHandler().Handle(
            new ApproveUnconfirmedIngredientCommand(stub.Id, IngredientTestData.ValidRequest()), CancellationToken.None);

        Assert.Equal(ResultStatus.Conflict, result.Status);
        _cache.DidNotReceive().Remove(Arg.Any<string>());
    }

    [Fact]
    public async Task Merge_ReturnsInvalid_WhenTheTargetIngredientDoesNotExist()
    {
        var stub = PendingStub();
        var handler = new MergeUnconfirmedIngredientCommandHandler(_reader, _writer, new FakeIngredientReader(null), TimeProvider.System);

        var result = await handler.Handle(new MergeUnconfirmedIngredientCommand(stub.Id, Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(ResultStatus.Invalid, result.Status);
        await _writer.DidNotReceiveWithAnyArgs().ResolveAsync(default, default, default, default, default);
    }

    [Fact]
    public async Task Merge_ResolvesAsMergedWithoutCreatingAnIngredient()
    {
        var stub = PendingStub();
        var target = IngredientMapper.ToIngredient(IngredientTestData.ValidRequest(), Guid.NewGuid(), isOfficial: false, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        _writer.ResolveAsync(default, default, default, default, default).ReturnsForAnyArgs(true);
        var handler = new MergeUnconfirmedIngredientCommandHandler(_reader, _writer, new FakeIngredientReader(target), TimeProvider.System);

        var result = await handler.Handle(new MergeUnconfirmedIngredientCommand(stub.Id, target.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(UnconfirmedIngredientStatus.Merged, result.Value!.ReviewStatus);
        Assert.Equal(target.Id, result.Value.ResolvedIngredientId);
        await _writer.Received(1).ResolveAsync(
            stub.Id, UnconfirmedIngredientStatus.Merged, target.Id, null, Arg.Any<DateTimeOffset>());
    }

    [Fact]
    public async Task Reject_StoresTheTrimmedReason_AndTreatsABlankOneAsNone()
    {
        var withReason = PendingStub();
        var blank = PendingStub();
        _writer.RejectAsync(default, default, default).ReturnsForAnyArgs(true);
        var handler = new RejectUnconfirmedIngredientCommandHandler(_reader, _writer, TimeProvider.System);

        var first = await handler.Handle(new RejectUnconfirmedIngredientCommand(withReason.Id, "  Finnes fra før  "), CancellationToken.None);
        var second = await handler.Handle(new RejectUnconfirmedIngredientCommand(blank.Id, "   "), CancellationToken.None);

        Assert.Equal("Finnes fra før", first.Value!.RejectionReason);
        Assert.Equal(UnconfirmedIngredientStatus.Rejected, first.Value.ReviewStatus);
        Assert.Null(second.Value!.RejectionReason);
    }

    [Fact]
    public async Task Reject_ReturnsConflict_ForARequestThatIsNotPending()
    {
        var stub = UnconfirmedIngredientTestData.Stub(Guid.NewGuid(), UnconfirmedIngredientStatus.Approved);
        _reader.GetByIdAsync(stub.Id).Returns(stub);

        var result = await new RejectUnconfirmedIngredientCommandHandler(_reader, _writer, TimeProvider.System)
            .Handle(new RejectUnconfirmedIngredientCommand(stub.Id, "nei"), CancellationToken.None);

        Assert.Equal(ResultStatus.Conflict, result.Status);
    }
}
