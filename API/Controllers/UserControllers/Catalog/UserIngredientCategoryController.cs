using Domain.Ingredients;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.UserControllers.Catalog;

[Route("api/user/ingredient-categories")]
[Authorize]
public class UserIngredientCategoryController(IMediator mediator) : ReadCatalogController<IngredientCategory>(mediator)
{
}
