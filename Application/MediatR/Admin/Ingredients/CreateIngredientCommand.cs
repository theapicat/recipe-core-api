using Application.Results;
using Domain.Ingredients;
using MediatR;

namespace Application.MediatR.Admin.Ingredients;

public record CreateIngredientCommand(IngredientRequest Ingredient) : IRequest<Result<Ingredient>>;
