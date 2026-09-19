using Domain.Ingredients;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.AdminControllers.Catalog;

[Route("api/admin/allergens")]
[Authorize(Roles = "admin")]
public class AdminAllergenController(IMediator mediator) : ReadWriteCatalogController<Allergen>(mediator)
{
}
