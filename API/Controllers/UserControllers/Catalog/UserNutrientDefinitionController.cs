using Domain.Ingredients;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.UserControllers.Catalog;

// Flat liste med ParentId - hierarkiet (opptil tre nivåer) bygges av klienten. Nøkkelen er Matvaretabellens tekstkode.
[Route("api/user/nutrient-definitions")]
[Authorize]
public class UserNutrientDefinitionController(IMediator mediator) : ReadCatalogController<NutrientDefinition, string>(mediator)
{
}
