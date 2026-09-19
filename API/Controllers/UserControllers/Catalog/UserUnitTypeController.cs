using Domain.Units;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.UserControllers.Catalog;

[Route("api/user/unit-types")]
[Authorize]
public class UserUnitTypeController(IMediator mediator) : ReadCatalogController<UnitType>(mediator)
{
}
