using Domain.Ingredients;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.UserControllers.Catalog;

// Flat liste sortert på SortOrder. Hvert stoff har gruppen nøstet inni (Group, med Group.ParentGroup for undergrupper) - det er
// ingen egne endepunkter for grupper. Nøkkelen er Matvaretabellens tekstkode.
[Route("api/user/nutrient-definitions")]
[Authorize]
public class UserNutrientDefinitionController(IMediator mediator) : ReadCatalogController<NutrientDefinition, string>(mediator)
{
}
