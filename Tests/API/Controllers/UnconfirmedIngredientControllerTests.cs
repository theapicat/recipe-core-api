using System.Security.Claims;
using API.Controllers.AdminControllers.UnconfirmedIngredients;
using API.Controllers.UserControllers.UnconfirmedIngredients;
using Application.MediatR.Admin.UnconfirmedIngredients;
using Application.MediatR.User.UnconfirmedIngredients;
using Application.Results;
using Domain.Ingredients;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Tests.API.Controllers;

public class UnconfirmedIngredientControllerTests
{
    private static UserUnconfirmedIngredientController UserController(IMediator mediator, Guid userId) => new(mediator)
    {
        ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId.ToString())], "test"))
            }
        }
    };

    // Kjerneregelen: eier-id kommer alltid fra tokenet, aldri fra request-body/query/rute.
    [Fact]
    public async Task Create_UsesTheUserIdFromTheToken_NotFromTheBody()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateUnconfirmedIngredientCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<UnconfirmedIngredient>.Success(new UnconfirmedIngredient
            {
                Id = Guid.NewGuid(), Name = "x", CreatedByUserId = userId,
                ReviewStatus = UnconfirmedIngredientStatus.NotRequested, CreatedAt = DateTimeOffset.UtcNow
            }));

        var response = await UserController(mediator, userId)
            .Create(new CreateUnconfirmedIngredientRequest("Lilla gulrot", true), CancellationToken.None);

        Assert.IsType<CreatedAtActionResult>(response);
        await mediator.Received(1).Send(
            Arg.Is<CreateUnconfirmedIngredientCommand>(c => c.UserId == userId && c.Name == "Lilla gulrot" && c.RequestReview),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAll_AsksForTheTokenUsersIngredientsOnly()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetOwnUnconfirmedIngredientsQuery>(), Arg.Any<CancellationToken>()).Returns([]);

        await UserController(mediator, userId).GetAll(CancellationToken.None);

        await mediator.Received(1).Send(
            Arg.Is<GetOwnUnconfirmedIngredientsQuery>(q => q.UserId == userId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_ForAnotherUsersIngredient()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteOwnUnconfirmedIngredientCommand>(), Arg.Any<CancellationToken>()).Returns(Result.NotFound());

        var response = await UserController(mediator, Guid.NewGuid()).Delete(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(response);
    }

    [Theory]
    [InlineData(null, false, UnconfirmedIngredientStatus.Pending)]
    [InlineData(UnconfirmedIngredientStatus.Rejected, false, UnconfirmedIngredientStatus.Rejected)]
    [InlineData(null, true, null)]
    [InlineData(UnconfirmedIngredientStatus.Rejected, true, null)]
    public async Task AdminQueue_DefaultsToPending_AndAllTrueRemovesTheFilter(
        UnconfirmedIngredientStatus? status, bool all, UnconfirmedIngredientStatus? expectedFilter)
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetUnconfirmedIngredientsForReviewQuery>(), Arg.Any<CancellationToken>()).Returns([]);
        var controller = new AdminUnconfirmedIngredientController(mediator);

        await controller.GetAll(status, all, CancellationToken.None);

        await mediator.Received(1).Send(
            Arg.Is<GetUnconfirmedIngredientsForReviewQuery>(q => q.Status == expectedFilter), Arg.Any<CancellationToken>());
    }
}
