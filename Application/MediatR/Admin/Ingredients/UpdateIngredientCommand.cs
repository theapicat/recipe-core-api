using Application.Results;
using Domain.Ingredients;
using MediatR;

namespace Application.MediatR.Admin.Ingredients;

// Erstatter hele ingrediensen, inkl. barna (barna får nye id-er - de er ikke stabile på tvers av oppdateringer).
public record UpdateIngredientCommand(Guid Id, IngredientRequest Ingredient) : IRequest<Result<Ingredient>>;
