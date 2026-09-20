using Application.MediatR.Admin.Ingredients;
using Application.Results;
using Domain.Ingredients;
using MediatR;

namespace Application.MediatR.Admin.UnconfirmedIngredients;

// Oppretter en ny offisiell ingrediens ut av forespørselen (evt. som variant ved at VariantOfIngredientId er satt i
// forespørselen) og avgjør den som Approved - i én transaksjon. Oppskriftslinjer flyttes til den nye ingrediensen.
public record ApproveUnconfirmedIngredientCommand(Guid Id, IngredientRequest Ingredient) : IRequest<Result<Ingredient>>;
