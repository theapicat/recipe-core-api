using Application.Results;
using Domain.Ingredients;
using MediatR;

namespace Application.MediatR.Admin.UnconfirmedIngredients;

// Forespørselen er en duplikat av en eksisterende ingrediens: ingen ny ingrediens opprettes, oppskriftslinjene
// flyttes til IngredientId og forespørselen avgjøres som Merged.
public record MergeUnconfirmedIngredientCommand(Guid Id, Guid IngredientId) : IRequest<Result<UnconfirmedIngredient>>;
