using API.Extensions;
using Application.MediatR.User.Recipes;
using Domain.Recipes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.UserControllers.Recipes;

// Brukerens egne oppskrifter. Alt filtreres på bruker-id fra tokenet - en annen brukers oppskrift gir samme 404 som en id som ikke
// finnes. Stegene og ingredienslinjene er en del av oppskriften (leses og skrives sammen med den) og har ingen egne endepunkter.
public class UserRecipeController(IMediator mediator) : UserController
{
    // Hele lista i lettvektsform - klienten filtrerer og søker (tittel, kategori, favoritt) selv.
    [HttpGet("recipes")]
    public async Task<ActionResult<List<RecipeListItem>>> GetAll(CancellationToken cancellationToken)
        => Ok(await mediator.Send(new GetOwnRecipesQuery(User.GetUserId()), cancellationToken));

    [HttpGet("recipes/{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetOwnRecipeQuery(User.GetUserId(), id), cancellationToken);
        return this.ToActionResult(result, Ok);
    }

    // Næring regnes ut på forespørsel fra dagens ingrediensdata (lagres ikke i oppskriften) og er veiledende. Kun næringsstoffer med
    // verdi er med. Hentes typisk når brukeren åpner næringsfanen.
    [HttpGet("recipes/{id:guid}/nutrition")]
    public async Task<IActionResult> GetNutrition(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetRecipeNutritionQuery(User.GetUserId(), id), cancellationToken);
        return this.ToActionResult(result, Ok);
    }

    // Svaret (201) er hele oppskriften med tildelte id-er og ingrediensnavn.
    [HttpPost("recipes")]
    public async Task<IActionResult> Create(RecipeRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateRecipeCommand(User.GetUserId(), request), cancellationToken);
        return this.ToActionResult(result, recipe => CreatedAtAction(nameof(GetById), new { id = recipe.Id }, recipe));
    }

    // Erstatter hele oppskriften, inkludert steg og ingredienser (de får nye id-er).
    [HttpPut("recipes/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, RecipeRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateRecipeCommand(User.GetUserId(), id, request), cancellationToken);
        return this.ToActionResult(result, Ok);
    }

    [HttpDelete("recipes/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteRecipeCommand(User.GetUserId(), id), cancellationToken);
        return this.ToActionResult(result, NoContent);
    }

    // Egen liten operasjon så favoritt kan slås av/på (f.eks. fra lista) uten å sende hele oppskriften på nytt.
    [HttpPut("recipes/{id:guid}/favorite")]
    public async Task<IActionResult> SetFavorite(Guid id, SetRecipeFavoriteRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new SetRecipeFavoriteCommand(User.GetUserId(), id, request.IsFavorite), cancellationToken);
        return this.ToActionResult(result, NoContent);
    }
}
