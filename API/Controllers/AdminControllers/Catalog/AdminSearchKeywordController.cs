using Domain.Ingredients;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.AdminControllers.Catalog;

[Route("api/admin/search-keywords")]
[Authorize(Roles = "admin")]
public class AdminSearchKeywordController(IMediator mediator) : ReadWriteCatalogController<SearchKeyword>(mediator)
{
}
