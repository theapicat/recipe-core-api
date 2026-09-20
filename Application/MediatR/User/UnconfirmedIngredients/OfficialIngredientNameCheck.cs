using Application.MediatR.Catalog;
using Domain.Ingredients;
using MediatR;

namespace Application.MediatR.User.UnconfirmedIngredients;

// En bruker skal ikke kunne opprette noe som allerede finnes i den offisielle katalogen. Sjekkes mot den cachede
// lettvekts-lista (gjenbruker cache-aside i GetAllCatalogQueryHandler). Navnet er allerede normalisert.
internal static class OfficialIngredientNameCheck
{
    public const string ExistsMessage = "En ingrediens med dette navnet finnes allerede i katalogen. Bruk den i stedet.";

    public static async Task<bool> ExistsAsync(IMediator mediator, string normalizedName, CancellationToken cancellationToken)
    {
        var official = await mediator.Send(new GetAllCatalogQuery<IngredientListItem>(), cancellationToken);
        return official.Any(i => string.Equals(i.Name, normalizedName, StringComparison.OrdinalIgnoreCase));
    }
}
