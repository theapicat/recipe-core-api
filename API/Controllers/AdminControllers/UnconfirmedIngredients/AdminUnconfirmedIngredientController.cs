using API.Extensions;
using Application.MediatR.Admin.Ingredients;
using Application.MediatR.Admin.UnconfirmedIngredients;
using Domain.Ingredients;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.AdminControllers.UnconfirmedIngredients;

// Admin-køen. Tre utfall for en ventende forespørsel: godkjenn (ny offisiell ingrediens, evt. som variant),
// slå sammen med en eksisterende ingrediens, eller avslå.
public class AdminUnconfirmedIngredientController(IMediator mediator) : AdminController
{
    // Uten filter vises ventende forespørsler. status=<verdi> filtrerer på en annen status, all=true gir alle.
    [HttpGet("unconfirmed-ingredients")]
    public async Task<ActionResult<List<UnconfirmedIngredient>>> GetAll(
        [FromQuery] UnconfirmedIngredientStatus? status,
        [FromQuery] bool all,
        CancellationToken cancellationToken)
    {
        UnconfirmedIngredientStatus? filter = all ? null : status ?? UnconfirmedIngredientStatus.Pending;
        return Ok(await mediator.Send(new GetUnconfirmedIngredientsForReviewQuery(filter), cancellationToken));
    }

    [HttpGet("unconfirmed-ingredients/{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUnconfirmedIngredientForReviewQuery(id), cancellationToken);
        return this.ToActionResult(result, Ok);
    }

    // Svaret er den nye ingrediensen med tildelte id-er.
    [HttpPost("unconfirmed-ingredients/{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, IngredientRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ApproveUnconfirmedIngredientCommand(id, request), cancellationToken);
        return this.ToActionResult(result, Ok);
    }

    [HttpPost("unconfirmed-ingredients/{id:guid}/merge")]
    public async Task<IActionResult> Merge(Guid id, MergeUnconfirmedIngredientRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new MergeUnconfirmedIngredientCommand(id, request.IngredientId), cancellationToken);
        return this.ToActionResult(result, Ok);
    }

    [HttpPost("unconfirmed-ingredients/{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, RejectUnconfirmedIngredientRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RejectUnconfirmedIngredientCommand(id, request.Reason), cancellationToken);
        return this.ToActionResult(result, Ok);
    }
}
