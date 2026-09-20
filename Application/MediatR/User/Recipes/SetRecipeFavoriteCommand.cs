using Application.Results;
using MediatR;

namespace Application.MediatR.User.Recipes;

public record SetRecipeFavoriteCommand(Guid UserId, Guid Id, bool IsFavorite) : IRequest<Result>;
