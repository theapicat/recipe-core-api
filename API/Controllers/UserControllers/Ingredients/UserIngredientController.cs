using Application.MediatR.Catalog;
using Application.MediatR.Ingredients;
using Domain.Ingredients;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.UserControllers.Ingredients;

// Søk går mot den cachede lettvekts-lista (filtrert i minnet). Enkeltoppslag gir den fulle ingrediensen, alltid fersk.
public class UserIngredientController(IMediator mediator) : UserController
{
    [HttpGet("ingredients")]
    public async Task<ActionResult<List<IngredientListItem>>> Search(
        [FromQuery] string? name,
        [FromQuery] Guid? categoryId,
        [FromQuery] Guid? allergenId,
        [FromQuery] Guid[]? excludeAllergenId,
        [FromQuery] Guid? searchKeywordId,
        CancellationToken cancellationToken)
        => Ok(await mediator.Send(
            new SearchIngredientsQuery(name, categoryId, allergenId, excludeAllergenId, searchKeywordId), cancellationToken));

    [HttpGet("ingredients/{id:guid}")]
    public async Task<ActionResult<Ingredient>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var ingredient = await mediator.Send(new GetCatalogByIdQuery<Ingredient, Guid>(id), cancellationToken);
        return ingredient is null ? NotFound() : Ok(ingredient);
    }
}
