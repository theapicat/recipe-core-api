using Application.Naming;
using Application.Results;
using Domain.Ingredients;
using MediatR;
using Persistence.Interfaces;

namespace Application.MediatR.User.UnconfirmedIngredients;

public class CreateUnconfirmedIngredientCommandHandler(
    IUnconfirmedIngredientReader reader,
    IUnconfirmedIngredientWriter writer,
    IMediator mediator,
    TimeProvider timeProvider)
    : IRequestHandler<CreateUnconfirmedIngredientCommand, Result<UnconfirmedIngredient>>
{
    public async Task<Result<UnconfirmedIngredient>> Handle(
        CreateUnconfirmedIngredientCommand request, CancellationToken cancellationToken)
    {
        var name = NameNormalizer.Normalize(request.Name);
        if (name.Length == 0 || name.Length > UnconfirmedIngredientLimits.MaxNameLength)
            return Result<UnconfirmedIngredient>.Invalid(
                $"Navnet må være mellom 1 og {UnconfirmedIngredientLimits.MaxNameLength} tegn.");

        if (await OfficialIngredientNameCheck.ExistsAsync(mediator, name, cancellationToken))
            return Result<UnconfirmedIngredient>.Conflict(OfficialIngredientNameCheck.ExistsMessage);

        if (await reader.CountByUserAsync(request.UserId) >= UnconfirmedIngredientLimits.MaxPerUser)
            return Result<UnconfirmedIngredient>.Conflict(
                $"Du kan ha maks {UnconfirmedIngredientLimits.MaxPerUser} egne ingredienser. Slett noen før du legger til flere.");

        if (request.RequestReview && await PendingLimitReachedAsync(request.UserId))
            return Result<UnconfirmedIngredient>.Conflict(
                $"Du kan ha maks {UnconfirmedIngredientLimits.MaxPendingPerUser} forespørsler til behandling om gangen.");

        var ingredient = new UnconfirmedIngredient
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            CreatedByUserId = request.UserId,
            ReviewStatus = request.RequestReview ? UnconfirmedIngredientStatus.Pending : UnconfirmedIngredientStatus.NotRequested,
            CreatedAt = timeProvider.GetUtcNow()
        };

        await writer.AddAsync(ingredient);
        return Result<UnconfirmedIngredient>.Success(ingredient);
    }

    private async Task<bool> PendingLimitReachedAsync(Guid userId) =>
        await reader.CountByUserAsync(userId, UnconfirmedIngredientStatus.Pending) >= UnconfirmedIngredientLimits.MaxPendingPerUser;
}
