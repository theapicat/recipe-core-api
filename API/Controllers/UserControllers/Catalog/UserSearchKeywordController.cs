using Domain.Ingredients;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.UserControllers.Catalog;

[Route("api/user/search-keywords")]
[Authorize]
public class UserSearchKeywordController(IMediator mediator) : ReadCatalogController<SearchKeyword>(mediator)
{
}
