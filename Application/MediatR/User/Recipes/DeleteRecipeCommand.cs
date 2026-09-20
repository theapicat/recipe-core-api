using Application.Results;
using MediatR;

namespace Application.MediatR.User.Recipes;

public record DeleteRecipeCommand(Guid UserId, Guid Id) : IRequest<Result>;
