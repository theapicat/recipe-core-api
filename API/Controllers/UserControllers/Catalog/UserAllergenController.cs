using Domain.Ingredients;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.UserControllers.Catalog;

[Route("api/user/allergens")]
[Authorize]
public class UserAllergenController(IMediator mediator) : ReadCatalogController<Allergen>(mediator)
{
}
