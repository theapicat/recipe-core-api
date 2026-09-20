using API.Extensions;
using Application.MediatR.User.UnconfirmedIngredients;
using Domain.Ingredients;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.UserControllers.UnconfirmedIngredients;

// Brukerens egne ubekreftede ingredienser. Alt filtreres på bruker-id fra tokenet - en annen brukers ingrediens
// gir samme 404 som en id som ikke finnes.
public class UserUnconfirmedIngredientController(IMediator mediator) : UserController
{
    [HttpGet("unconfirmed-ingredients")]
    public async Task<ActionResult<List<UnconfirmedIngredient>>> GetAll(CancellationToken cancellationToken)
        => Ok(await mediator.Send(new GetOwnUnconfirmedIngredientsQuery(User.GetUserId()), cancellationToken));

    [HttpGet("unconfirmed-ingredients/{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetOwnUnconfirmedIngredientQuery(User.GetUserId(), id), cancellationToken);
        return this.ToActionResult(result, Ok);
    }

    [HttpPost("unconfirmed-ingredients")]
    public async Task<IActionResult> Create(CreateUnconfirmedIngredientRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateUnconfirmedIngredientCommand(User.GetUserId(), request.Name, request.RequestReview), cancellationToken);
        return this.ToActionResult(result, created => CreatedAtAction(nameof(GetById), new { id = created.Id }, created));
    }

    [HttpPut("unconfirmed-ingredients/{id:guid}")]
    public async Task<IActionResult> Rename(Guid id, RenameUnconfirmedIngredientRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new RenameOwnUnconfirmedIngredientCommand(User.GetUserId(), id, request.Name), cancellationToken);
        return this.ToActionResult(result, Ok);
    }

    [HttpPost("unconfirmed-ingredients/{id:guid}/request-review")]
    public async Task<IActionResult> RequestReview(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RequestReviewOwnUnconfirmedIngredientCommand(User.GetUserId(), id), cancellationToken);
        return this.ToActionResult(result, Ok);
    }

    [HttpDelete("unconfirmed-ingredients/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteOwnUnconfirmedIngredientCommand(User.GetUserId(), id), cancellationToken);
        return this.ToActionResult(result, NoContent);
    }
}
