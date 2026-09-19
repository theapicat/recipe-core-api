using Domain.Recipes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.UserControllers.Catalog;

[Route("api/user/recipe-categories")]
[Authorize]
public class UserRecipeCategoryController(IMediator mediator) : ReadCatalogController<RecipeCategory>(mediator)
{
}
