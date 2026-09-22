using Application.Caching.Interfaces;
using Application.MediatR.Catalog;
using Application.Results;
using Domain.Ingredients;
using MediatR;
using Persistence.Services;

namespace Application.MediatR.Admin.Ingredients;

public class UpdateIngredientCommandHandler(
    DbReader<Ingredient> reader, DbWriter<Ingredient> writer, ICacheService cache, IMediator mediator, TimeProvider timeProvider)
    : IRequestHandler<UpdateIngredientCommand, Result<Ingredient>>
{
    public async Task<Result<Ingredient>> Handle(UpdateIngredientCommand request, CancellationToken cancellationToken)
    {
        var error = IngredientMapper.Validate(request.Ingredient);
        if (error is not null)
            return Result<Ingredient>.Invalid(error);

        var existing = await reader.GetByIdAsync(request.Id);
        if (existing is null)
            return Result<Ingredient>.NotFound();

        // Optimistisk samtidighetskontroll: klienten sender tilbake tidspunktet fra sin siste GET. Utelatt = ingen sjekk.
        if (request.Ingredient.UpdatedAt is { } clientUpdatedAt && clientUpdatedAt < existing.UpdatedAt)
            return Result<Ingredient>.Conflict("Ingrediensen er endret av noen andre siden du åpnet den. Last den på nytt.");

        if (existing.IsOfficial)
        {
            var lockError = IngredientMapper.ValidateOfficialLock(existing, request.Ingredient);
            if (lockError is not null)
                return Result<Ingredient>.Invalid(lockError);
        }

        var fkError = await IngredientForeignKeyValidator.ValidateAsync(mediator, request.Ingredient, request.Id, cancellationToken);
        if (fkError is not null)
            return Result<Ingredient>.Invalid(fkError);

        // IsOfficial og CreatedAt videreføres fra den lagrede raden - kan aldri settes/endres via body.
        var ingredient = IngredientMapper.ToIngredient(
            request.Ingredient, request.Id, existing.IsOfficial, timeProvider.GetUtcNow(), createdAt: existing.CreatedAt);
        await writer.UpdateAsync(ingredient);
        cache.Remove(CatalogCacheKey.ForAll<IngredientListItem>());

        return Result<Ingredient>.Success(ingredient);
    }
}
