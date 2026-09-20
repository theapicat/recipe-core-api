using API.Extensions;
using Application.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using NSubstitute;
using Xunit;

namespace Tests.API.Extensions;

public class ResultExtensionsTests
{
    private class TestController : ControllerBase;

    private static TestController CreateController()
    {
        var factory = Substitute.For<ProblemDetailsFactory>();
        factory.CreateProblemDetails(Arg.Any<HttpContext>(), Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<string?>(), Arg.Any<string?>())
            .Returns(call => new ProblemDetails { Status = call.ArgAt<int?>(1), Detail = call.ArgAt<string?>(4) });

        return new TestController { ProblemDetailsFactory = factory };
    }

    [Fact]
    public void Success_UsesTheSuccessMapping()
    {
        var controller = CreateController();

        var response = controller.ToActionResult(Result<string>.Success("ok"), value => controller.Ok(value));

        Assert.Equal("ok", Assert.IsType<OkObjectResult>(response).Value);
    }

    [Fact]
    public void NotFound_MapsTo404()
    {
        var controller = CreateController();

        Assert.IsType<NotFoundResult>(controller.ToActionResult(Result<string>.NotFound(), _ => controller.Ok()));
    }

    [Fact]
    public void Conflict_MapsTo409WithTheMessage()
    {
        var controller = CreateController();

        var response = Assert.IsType<ObjectResult>(controller.ToActionResult(Result<string>.Conflict("i bruk"), _ => controller.Ok()));

        Assert.Equal(StatusCodes.Status409Conflict, response.StatusCode);
        Assert.Equal("i bruk", Assert.IsType<ProblemDetails>(response.Value).Detail);
    }

    [Fact]
    public void Invalid_MapsTo400_ForBothResultShapes()
    {
        var controller = CreateController();

        var generic = Assert.IsType<ObjectResult>(controller.ToActionResult(Result<string>.Invalid("feil"), _ => controller.Ok()));
        var plain = Assert.IsType<ObjectResult>(controller.ToActionResult(Result.Invalid("feil"), () => controller.Ok()));

        Assert.Equal(StatusCodes.Status400BadRequest, generic.StatusCode);
        Assert.Equal(StatusCodes.Status400BadRequest, plain.StatusCode);
    }
}
