using Domain.Units;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.AdminControllers.Catalog;

[Route("api/admin/unit-types")]
[Authorize(Roles = "admin")]
public class AdminUnitTypeController(IMediator mediator) : ReadWriteCatalogController<UnitType>(mediator)
{
}
