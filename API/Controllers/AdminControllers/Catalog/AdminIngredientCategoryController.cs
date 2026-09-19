using Domain.Ingredients;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.AdminControllers.Catalog;

[Route("api/admin/ingredient-categories")]
[Authorize(Roles = "admin")]
public class AdminIngredientCategoryController(IMediator mediator) : ReadWriteCatalogController<IngredientCategory>(mediator)
{
}
