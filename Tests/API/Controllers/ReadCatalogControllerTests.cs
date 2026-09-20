using API.Controllers.UserControllers.Catalog;
using Application.MediatR.Catalog;
using Domain.Ingredients;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Tests.API.Controllers;

public class ReadCatalogControllerTests
{
    [Fact]
    public async Task GetAll_ReturnsOkWithMediatorResult()
    {
        var mediator = Substitute.For<IMediator>();
        var items = new List<Allergen> { new() { Id = Guid.NewGuid(), Name = "Gluten" } };
        mediator.Send(Arg.Any<GetAllCatalogQuery<Allergen>>(), Arg.Any<CancellationToken>()).Returns(items);
        var controller = new UserAllergenController(mediator);

        var result = await controller.GetAll(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(items, okResult.Value);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenFound()
    {
        var mediator = Substitute.For<IMediator>();
        var item = new Allergen { Id = Guid.NewGuid(), Name = "Gluten" };
        mediator.Send(Arg.Any<GetCatalogByIdQuery<Allergen, Guid>>(), Arg.Any<CancellationToken>()).Returns(item);
        var controller = new UserAllergenController(mediator);

        var result = await controller.GetById(item.Id, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(item, okResult.Value);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetCatalogByIdQuery<Allergen, Guid>>(), Arg.Any<CancellationToken>()).Returns((Allergen?)null);
        var controller = new UserAllergenController(mediator);

        var result = await controller.GetById(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }
}
