using Domain.Ingredients;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.AdminControllers.Catalog;

[Route("api/admin/nutrient-definitions")]
[Authorize(Roles = "admin")]
public class AdminNutrientDefinitionController(IMediator mediator) : ReadWriteCatalogController<NutrientDefinition, string>(mediator)
{
}
