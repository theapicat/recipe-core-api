using Domain.Recipes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.AdminControllers.Catalog;

[Route("api/admin/recipe-categories")]
[Authorize(Roles = "admin")]
public class AdminRecipeCategoryController(IMediator mediator) : ReadWriteCatalogController<RecipeCategory>(mediator)
{
}
