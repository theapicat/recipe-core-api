using Application.Naming;
using Application.Results;
using Domain.Ingredients;
using MediatR;
using Persistence.Interfaces;

namespace Application.MediatR.User.UnconfirmedIngredients;

public class RenameOwnUnconfirmedIngredientCommandHandler(
    IUnconfirmedIngredientReader reader,
    IUnconfirmedIngredientWriter writer,
    IMediator mediator)
    : IRequestHandler<RenameOwnUnconfirmedIngredientCommand, Result<UnconfirmedIngredient>>
{
    public async Task<Result<UnconfirmedIngredient>> Handle(
        RenameOwnUnconfirmedIngredientCommand request, CancellationToken cancellationToken)
    {
        var name = NameNormalizer.Normalize(request.Name);
        if (name.Length == 0 || name.Length > UnconfirmedIngredientLimits.MaxNameLength)
            return Result<UnconfirmedIngredient>.Invalid(
                $"Navnet må være mellom 1 og {UnconfirmedIngredientLimits.MaxNameLength} tegn.");

        var ingredient = await reader.GetByIdAsync(request.Id);
        if (ingredient is null || ingredient.CreatedByUserId != request.UserId)
            return Result<UnconfirmedIngredient>.NotFound();

        if (ingredient.ReviewStatus != UnconfirmedIngredientStatus.NotRequested)
            return Result<UnconfirmedIngredient>.Conflict("Ingrediensen kan bare endres før en forespørsel er sendt.");

        if (await OfficialIngredientNameCheck.ExistsAsync(mediator, name, cancellationToken))
            return Result<UnconfirmedIngredient>.Conflict(OfficialIngredientNameCheck.ExistsMessage);

        if (!await writer.RenameAsync(request.Id, request.UserId, name))
            return Result<UnconfirmedIngredient>.Conflict("Ingrediensen kunne ikke endres.");

        ingredient.Name = name;
        return Result<UnconfirmedIngredient>.Success(ingredient);
    }
}
