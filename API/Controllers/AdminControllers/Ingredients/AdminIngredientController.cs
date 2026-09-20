using API.Extensions;
using Application.MediatR.Admin.Ingredients;
using Application.MediatR.Catalog;
using Application.MediatR.Ingredients;
using Domain.Ingredients;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.AdminControllers.Ingredients;

// Full CRUD. Lesing (søk/enkeltoppslag) er lik brukerens. Skriving tar IngredientRequest uten id-er - serveren
// tildeler id for ingrediensen og alle barna, og svaret inneholder hele ingrediensen med id-ene.
public class AdminIngredientController(IMediator mediator) : AdminController
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

    [HttpPost("ingredients")]
    public async Task<IActionResult> Create(IngredientRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateIngredientCommand(request), cancellationToken);
        return this.ToActionResult(result, ingredient => CreatedAtAction(nameof(GetById), new { id = ingredient.Id }, ingredient));
    }

    [HttpPut("ingredients/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, IngredientRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateIngredientCommand(id, request), cancellationToken);
        return this.ToActionResult(result, Ok);
    }

    [HttpDelete("ingredients/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteIngredientCommand(id), cancellationToken);
        return this.ToActionResult(result, NoContent);
    }
}
