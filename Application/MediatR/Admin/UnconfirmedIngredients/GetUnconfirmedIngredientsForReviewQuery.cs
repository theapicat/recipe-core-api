using Domain.Ingredients;
using MediatR;

namespace Application.MediatR.Admin.UnconfirmedIngredients;

// Admin-køen. Status = null gir alle statuser (eldste først).
public record GetUnconfirmedIngredientsForReviewQuery(UnconfirmedIngredientStatus? Status)
    : IRequest<List<UnconfirmedIngredient>>;
