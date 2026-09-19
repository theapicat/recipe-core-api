using API.Controllers.AdminControllers.Catalog;
using Application.MediatR.Catalog;
using Domain.Ingredients;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Tests.API.Controllers;

public class ReadWriteCatalogControllerTests
{
    [Fact]
    public async Task Insert_ReturnsOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<InsertCatalogCommand<Allergen>>(), Arg.Any<CancellationToken>()).Returns(true);
        var controller = new AdminAllergenController(mediator);
        var entity = new Allergen { Id = Guid.NewGuid(), Name = "Gluten" };

        var result = await controller.Insert(entity, CancellationToken.None);

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task Update_ReturnsOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateCatalogCommand<Allergen>>(), Arg.Any<CancellationToken>()).Returns(true);
        var controller = new AdminAllergenController(mediator);
        var entity = new Allergen { Id = Guid.NewGuid(), Name = "Gluten" };

        var result = await controller.Update(entity, CancellationToken.None);

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteCatalogCommand<Allergen>>(), Arg.Any<CancellationToken>()).Returns(true);
        var controller = new AdminAllergenController(mediator);

        var result = await controller.Delete(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }
}
