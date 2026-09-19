using Application.MediatR.Catalog;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public abstract class ReadWriteCatalogController<T>(IMediator mediator) : ReadCatalogController<T>(mediator)
{
    [HttpPost]
    public async Task<IActionResult> Insert(T entity, CancellationToken cancellationToken)
    {
        await Mediator.Send(new InsertCatalogCommand<T>(entity), cancellationToken);
        return Ok();
    }

    [HttpPut]
    public async Task<IActionResult> Update(T entity, CancellationToken cancellationToken)
    {
        await Mediator.Send(new UpdateCatalogCommand<T>(entity), cancellationToken);
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new DeleteCatalogCommand<T>(id), cancellationToken);
        return NoContent();
    }
}
