using MediatR;
using Microsoft.Extensions.Logging;
using Persistence.Interfaces;

namespace Application.MediatR.Users;

public class DeleteAllUserDataCommandHandler(IUserDataEraser eraser, ILogger<DeleteAllUserDataCommandHandler> logger)
    : IRequestHandler<DeleteAllUserDataCommand>
{
    public async Task Handle(DeleteAllUserDataCommand request, CancellationToken cancellationToken)
    {
        var result = await eraser.DeleteAllForUserAsync(request.UserId);

        logger.LogInformation(
            "Slettet alt data for bruker {UserId} etter kontosletting: {RecipesDeleted} oppskrifter, {UnconfirmedIngredientsDeleted} ubekreftede ingredienser.",
            request.UserId, result.RecipesDeleted, result.UnconfirmedIngredientsDeleted);
    }
}
