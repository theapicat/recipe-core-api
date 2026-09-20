using Application.Results;
using Domain.Ingredients;
using MediatR;

namespace Application.MediatR.Admin.UnconfirmedIngredients;

// Avslår en ventende forespørsel (terminal). Ingrediensen forblir privat hos brukeren, med begrunnelsen synlig.
public record RejectUnconfirmedIngredientCommand(Guid Id, string? Reason) : IRequest<Result<UnconfirmedIngredient>>;
