using Application.MediatR.Catalog;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

// Bevisst ikke avledet fra UserController/AdminController - de bærer rute/tilgangs-attributter
// (én akse), mens denne bærer CQRS-formen (en annen akse), og C# tillater bare én baseklasse.
// Den konkrete kontrolleren (f.eks. UserAllergenController) setter selv [Route]/[Authorize].
[ApiController]
public abstract class ReadCatalogController<T>(IMediator mediator) : ControllerBase
{
    protected readonly IMediator Mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<List<T>>> GetAll(CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetAllCatalogQuery<T>(), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<T>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetCatalogByIdQuery<T>(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
