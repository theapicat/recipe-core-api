using Application.Results;
using Domain.Ingredients;
using MediatR;

namespace Application.MediatR.Admin.UnconfirmedIngredients;

public record GetUnconfirmedIngredientForReviewQuery(Guid Id) : IRequest<Result<UnconfirmedIngredient>>;
