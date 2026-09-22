using API.Controllers;
using API.Controllers.AdminControllers.Catalog;
using Application.MediatR.Catalog;
using Application.Results;
using Domain;
using Domain.Ingredients;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using NSubstitute;
using Xunit;

namespace Tests.API.Controllers;

public class ReadWriteCatalogControllerTests
{
    private class TextKeyEntity : IHasId<string>, IHasUsageMetadata
    {
        public required string Id { get; set; }
        public bool IsSystem { get; set; }
        public int UsageCount { get; set; }
    }

    private class TextKeyController(IMediator mediator) : ReadWriteCatalogController<TextKeyEntity, string>(mediator);

    // ControllerBase.Problem() trenger en ProblemDetailsFactory - i en enhetstest finnes ingen DI, så en enkel stub settes inn.
    private static ProblemDetailsFactory Factory()
    {
        var factory = Substitute.For<ProblemDetailsFactory>();
        factory.CreateProblemDetails(Arg.Any<HttpContext>(), Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<string?>(), Arg.Any<string?>())
            .Returns(call => new ProblemDetails { Status = call.ArgAt<int?>(1), Detail = call.ArgAt<string?>(4) });
        return factory;
    }

    private static AdminAllergenController AllergenController(IMediator mediator) =>
        new(mediator) { ProblemDetailsFactory = Factory() };

    [Fact]
    public async Task Insert_ReturnsCreated_WithTheServerAssignedId()
    {
        var mediator = Substitute.For<IMediator>();
        var newId = Guid.NewGuid();
        mediator.Send(Arg.Any<InsertCatalogCommand<Allergen, Guid>>(), Arg.Any<CancellationToken>()).Returns(Result<Guid>.Success(newId));
        var controller = AllergenController(mediator);
        var entity = new Allergen { Name = "Gluten" };

        var result = await controller.Insert(entity, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(AdminAllergenController.GetById), created.ActionName);
        Assert.Equal(newId, created.RouteValues!["id"]);
        Assert.Same(entity, created.Value);
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenIdIsPresent()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateCatalogCommand<Allergen>>(), Arg.Any<CancellationToken>()).Returns(Result.Success());
        var controller = AllergenController(mediator);

        var result = await controller.Update(new Allergen { Id = Guid.NewGuid(), Name = "Gluten" }, CancellationToken.None);

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenIdIsMissing()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = AllergenController(mediator);

        var result = await controller.Update(new Allergen { Name = "Gluten" }, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
        await mediator.DidNotReceiveWithAnyArgs().Send<Result>(default!, default);
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenStringKeyIsBlank()
    {
        var controller = new TextKeyController(Substitute.For<IMediator>()) { ProblemDetailsFactory = Factory() };

        var result = await controller.Update(new TextKeyEntity { Id = " " }, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Insert_ReturnsBadRequest_WhenTheNameIsBlank(string name)
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<InsertCatalogCommand<Allergen, Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Guid>.Invalid("Navn må oppgis."));
        var controller = AllergenController(mediator);

        var result = await controller.Insert(new Allergen { Name = name }, CancellationToken.None);

        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenTheNameIsBlank()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateCatalogCommand<Allergen>>(), Arg.Any<CancellationToken>())
            .Returns(Result.Invalid("Navn må oppgis."));
        var controller = AllergenController(mediator);

        var result = await controller.Update(new Allergen { Id = Guid.NewGuid(), Name = " " }, CancellationToken.None);

        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteCatalogCommand<Allergen, Guid>>(), Arg.Any<CancellationToken>()).Returns(Result.Success());
        var controller = new AdminAllergenController(mediator);

        var result = await controller.Delete(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenTheRowDoesNotExist()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteCatalogCommand<Allergen, Guid>>(), Arg.Any<CancellationToken>()).Returns(Result.NotFound());
        var controller = new AdminAllergenController(mediator);

        var result = await controller.Delete(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsConflict_WhenTheRowIsSystemOrInUse()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteCatalogCommand<Allergen, Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.Conflict("Systemrader (fra seed-data) kan ikke slettes."));
        var controller = AllergenController(mediator);

        var result = await controller.Delete(Guid.NewGuid(), CancellationToken.None);

        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status409Conflict, problem.StatusCode);
    }
}
