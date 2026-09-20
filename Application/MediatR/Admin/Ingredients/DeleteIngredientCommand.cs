using Application.Results;
using MediatR;

namespace Application.MediatR.Admin.Ingredients;

public record DeleteIngredientCommand(Guid Id) : IRequest<Result>;
