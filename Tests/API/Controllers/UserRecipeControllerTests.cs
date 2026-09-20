using System.Security.Claims;
using API.Controllers.UserControllers.Recipes;
using Application.MediatR.User.Recipes;
using Application.Results;
using Domain.Recipes;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using NSubstitute;
using Tests.Application.MediatR.User.Recipes;
using Xunit;

namespace Tests.API.Controllers;

public class UserRecipeControllerTests
{
    // ControllerBase.Problem() trenger en ProblemDetailsFactory - i en enhetstest finnes ingen DI, så en enkel stub settes inn.
    private static ProblemDetailsFactory Factory()
    {
        var factory = Substitute.For<ProblemDetailsFactory>();
        factory.CreateProblemDetails(Arg.Any<HttpContext>(), Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<string?>(), Arg.Any<string?>())
            .Returns(call => new ProblemDetails { Status = call.ArgAt<int?>(1), Detail = call.ArgAt<string?>(4) });
        return factory;
    }

    private static UserRecipeController Controller(IMediator mediator, Guid userId) => new(mediator)
    {
        ProblemDetailsFactory = Factory(),
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
    public async Task Create_UsesTheUserIdFromTheToken_AndReturnsCreated()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        var recipe = RecipeTestData.ExistingRecipe(userId);
        mediator.Send(Arg.Any<CreateRecipeCommand>(), Arg.Any<CancellationToken>()).Returns(Result<Recipe>.Success(recipe));

        var result = await Controller(mediator, userId).Create(RecipeTestData.ValidRequest(), CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(UserRecipeController.GetById), created.ActionName);
        Assert.Equal(recipe.Id, created.RouteValues!["id"]);
        await mediator.Received(1).Send(Arg.Is<CreateRecipeCommand>(c => c.UserId == userId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAll_AsksForTheTokenUsersList()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetOwnRecipesQuery>(), Arg.Any<CancellationToken>()).Returns(new List<RecipeListItem>());

        await Controller(mediator, userId).GetAll(CancellationToken.None);

        await mediator.Received(1).Send(Arg.Is<GetOwnRecipesQuery>(q => q.UserId == userId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetNutrition_UsesTheTokenUser_AndReturnsOk()
    {
        var userId = Guid.NewGuid();
        var id = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetRecipeNutritionQuery>(), Arg.Any<CancellationToken>()).Returns(Result<RecipeNutrition>.Success(
            new RecipeNutrition { RecipeId = id, Servings = 2, Nutrients = [], CountedIngredients = 0, TotalIngredients = 0, SkippedLines = [] }));

        var result = await Controller(mediator, userId).GetNutrition(id, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        await mediator.Received(1).Send(Arg.Is<GetRecipeNutritionQuery>(q => q.UserId == userId && q.Id == id), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetNutrition_Returns404_WhenTheRecipeIsNotTheUsers()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetRecipeNutritionQuery>(), Arg.Any<CancellationToken>()).Returns(Result<RecipeNutrition>.NotFound());

        var result = await Controller(mediator, Guid.NewGuid()).GetNutrition(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetById_Returns404_WhenTheMediatorSaysNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetOwnRecipeQuery>(), Arg.Any<CancellationToken>()).Returns(Result<Recipe>.NotFound());

        var result = await Controller(mediator, Guid.NewGuid()).GetById(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_Returns409_WhenTheLimitIsReached()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateRecipeCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<Recipe>.Conflict("Du kan ha maks 500 oppskrifter."));

        var result = await Controller(mediator, Guid.NewGuid()).Create(RecipeTestData.ValidRequest(), CancellationToken.None);

        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status409Conflict, problem.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_OnSuccess_AndUsesTheTokenUser()
    {
        var userId = Guid.NewGuid();
        var id = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteRecipeCommand>(), Arg.Any<CancellationToken>()).Returns(Result.Success());

        var result = await Controller(mediator, userId).Delete(id, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        await mediator.Received(1).Send(Arg.Is<DeleteRecipeCommand>(c => c.UserId == userId && c.Id == id), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetFavorite_UsesTheTokenUser_AndReturnsNoContent()
    {
        var userId = Guid.NewGuid();
        var id = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<SetRecipeFavoriteCommand>(), Arg.Any<CancellationToken>()).Returns(Result.Success());

        var result = await Controller(mediator, userId).SetFavorite(id, new SetRecipeFavoriteRequest(true), CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        await mediator.Received(1).Send(
            Arg.Is<SetRecipeFavoriteCommand>(c => c.UserId == userId && c.Id == id && c.IsFavorite), Arg.Any<CancellationToken>());
    }
}
