using Application.Results;
using MediatR;

namespace Application.MediatR.User.UnconfirmedIngredients;

public record DeleteOwnUnconfirmedIngredientCommand(Guid UserId, Guid Id) : IRequest<Result>;
