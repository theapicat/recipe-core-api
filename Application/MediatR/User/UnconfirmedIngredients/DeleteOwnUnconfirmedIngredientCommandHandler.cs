using Application.Results;
using MediatR;
using Persistence.Interfaces;

namespace Application.MediatR.User.UnconfirmedIngredients;

public class DeleteOwnUnconfirmedIngredientCommandHandler(IUnconfirmedIngredientWriter writer)
    : IRequestHandler<DeleteOwnUnconfirmedIngredientCommand, Result>
{
    public async Task<Result> Handle(DeleteOwnUnconfirmedIngredientCommand request, CancellationToken cancellationToken)
        => await writer.DeleteOwnAsync(request.Id, request.UserId) ? Result.Success() : Result.NotFound();
}
