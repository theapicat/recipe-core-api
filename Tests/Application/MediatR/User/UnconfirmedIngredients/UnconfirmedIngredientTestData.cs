using Domain.Ingredients;

namespace Tests.Application.MediatR.User.UnconfirmedIngredients;

internal static class UnconfirmedIngredientTestData
{
    public static UnconfirmedIngredient Stub(Guid userId, UnconfirmedIngredientStatus status = UnconfirmedIngredientStatus.NotRequested) => new()
    {
        Id = Guid.NewGuid(),
        Name = "Lilla gulrot",
        CreatedByUserId = userId,
        ReviewStatus = status,
        CreatedAt = DateTimeOffset.UtcNow
    };
}
