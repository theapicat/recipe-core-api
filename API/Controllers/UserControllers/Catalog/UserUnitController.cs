using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DomainUnit = Domain.Units.Unit;

namespace API.Controllers.UserControllers.Catalog;

[Route("api/user/units")]
[Authorize]
public class UserUnitController(IMediator mediator) : ReadCatalogController<DomainUnit>(mediator)
{
}
