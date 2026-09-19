using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DomainUnit = Domain.Units.Unit;

namespace API.Controllers.AdminControllers.Catalog;

[Route("api/admin/units")]
[Authorize(Roles = "admin")]
public class AdminUnitController(IMediator mediator) : ReadWriteCatalogController<DomainUnit>(mediator)
{
}
