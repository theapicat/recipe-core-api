using API.Extensions;
using Application.MediatR.Catalog;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public abstract class ReadWriteCatalogController<T, TKey>(IMediator mediator) : ReadCatalogController<T, TKey>(mediator)
    where T : IHasId<TKey>
{
    // Serveren tildeler id for Guid-nøkler (klientens id ignoreres). Svaret er 201 med den opprettede raden og en
    // Location-header, så klienten alltid får id-en tilbake. Virtual slik at en konkret kontroller (f.eks. enheter) kan
    // erstatte den generiske kommandoen med en egen som har ekstra forretningsregler.
    [HttpPost]
    public virtual async Task<IActionResult> Insert(T entity, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new InsertCatalogCommand<T, TKey>(entity), cancellationToken);
        return this.ToActionResult(result, id => CreatedAtAction(nameof(GetById), new { id }, entity));
    }

    [HttpPut]
    public virtual async Task<IActionResult> Update(T entity, CancellationToken cancellationToken)
    {
        if (IsMissing(entity.Id))
            return BadRequest("Id må oppgis.");

        var result = await Mediator.Send(new UpdateCatalogCommand<T>(entity), cancellationToken);
        return this.ToActionResult(result, Ok);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(TKey id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new DeleteCatalogCommand<T, TKey>(id), cancellationToken);
        return NoContent();
    }

    private static bool IsMissing(TKey id) =>
        EqualityComparer<TKey>.Default.Equals(id, default!) || (id is string text && string.IsNullOrWhiteSpace(text));
}

// Snarvei for det vanlige tilfellet: Guid-nøkkel.
public abstract class ReadWriteCatalogController<T>(IMediator mediator) : ReadWriteCatalogController<T, Guid>(mediator)
    where T : IHasId<Guid>;
