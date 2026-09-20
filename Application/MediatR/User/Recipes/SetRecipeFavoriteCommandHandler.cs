using Application.Results;
using MediatR;
using Persistence.Interfaces;

namespace Application.MediatR.User.Recipes;

public class SetRecipeFavoriteCommandHandler(IRecipeWriter writer) : IRequestHandler<SetRecipeFavoriteCommand, Result>
{
    public async Task<Result> Handle(SetRecipeFavoriteCommand command, CancellationToken cancellationToken) =>
        await writer.SetFavoriteAsync(command.Id, command.UserId, command.IsFavorite) ? Result.Success() : Result.NotFound();
}
