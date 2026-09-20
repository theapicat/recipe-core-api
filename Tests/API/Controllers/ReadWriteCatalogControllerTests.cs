using API.Controllers;
using API.Controllers.AdminControllers.Catalog;
using Application.MediatR.Catalog;
using Domain;
using Domain.Ingredients;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Tests.API.Controllers;

public class ReadWriteCatalogControllerTests
{
    private class TextKeyEntity : IHasId<string>
    {
        public required string Id { get; set; }
    }

    private class TextKeyController(IMediator mediator) : ReadWriteCatalogController<TextKeyEntity, string>(mediator);

    [Fact]
    public async Task Insert_ReturnsCreated_WithTheServerAssignedId()
    {
        var mediator = Substitute.For<IMediator>();
        var newId = Guid.NewGuid();
        mediator.Send(Arg.Any<InsertCatalogCommand<Allergen, Guid>>(), Arg.Any<CancellationToken>()).Returns(newId);
        var controller = new AdminAllergenController(mediator);
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
        mediator.Send(Arg.Any<UpdateCatalogCommand<Allergen>>(), Arg.Any<CancellationToken>()).Returns(true);
        var controller = new AdminAllergenController(mediator);

        var result = await controller.Update(new Allergen { Id = Guid.NewGuid(), Name = "Gluten" }, CancellationToken.None);

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenIdIsMissing()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = new AdminAllergenController(mediator);

        var result = await controller.Update(new Allergen { Name = "Gluten" }, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
        await mediator.DidNotReceiveWithAnyArgs().Send<bool>(default!, default);
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenStringKeyIsBlank()
    {
        var controller = new TextKeyController(Substitute.For<IMediator>());

        var result = await controller.Update(new TextKeyEntity { Id = " " }, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Insert_ReturnsBadRequest_WhenTheNameIsBlank(string name)
    {
        var mediator = Substitute.For<IMediator>();
        var controller = new AdminAllergenController(mediator);

        var result = await controller.Insert(new Allergen { Name = name }, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
        await mediator.DidNotReceiveWithAnyArgs().Send<Guid>(default!, default);
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenTheNameIsBlank()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = new AdminAllergenController(mediator);

        var result = await controller.Update(new Allergen { Id = Guid.NewGuid(), Name = " " }, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
        await mediator.DidNotReceiveWithAnyArgs().Send<bool>(default!, default);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteCatalogCommand<Allergen, Guid>>(), Arg.Any<CancellationToken>()).Returns(true);
        var controller = new AdminAllergenController(mediator);

        var result = await controller.Delete(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }
}
