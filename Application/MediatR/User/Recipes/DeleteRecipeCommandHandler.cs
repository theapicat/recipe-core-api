using Application.Results;
using MediatR;
using Persistence.Interfaces;

namespace Application.MediatR.User.Recipes;

public class DeleteRecipeCommandHandler(IRecipeWriter writer) : IRequestHandler<DeleteRecipeCommand, Result>
{
    public async Task<Result> Handle(DeleteRecipeCommand command, CancellationToken cancellationToken) =>
        await writer.DeleteAsync(command.Id, command.UserId) ? Result.Success() : Result.NotFound();
}
