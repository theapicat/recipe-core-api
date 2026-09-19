using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("/api/admin")]
[Authorize(Roles = "admin")]
public abstract class AdminController : ControllerBase
{
}
