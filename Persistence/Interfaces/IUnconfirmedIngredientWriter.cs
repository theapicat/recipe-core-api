using Domain.Ingredients;

namespace Persistence.Interfaces;

// Metodene som endrer en eksisterende rad returnerer false når ingen rad matchet vilkåret (finnes ikke,
// feil eier eller feil status) - kalleren avgjør hvordan det oversettes.
public interface IUnconfirmedIngredientWriter
{
    Task AddAsync(UnconfirmedIngredient entity);

    Task<bool> RenameAsync(Guid id, Guid userId, string name);

    Task<bool> RequestReviewAsync(Guid id, Guid userId);

    Task<bool> DeleteOwnAsync(Guid id, Guid userId);

    Task<bool> RejectAsync(Guid id, string? reason, DateTimeOffset reviewedAt);

    // Avgjør en ventende forespørsel som Approved eller Merged og flytter oppskriftslinjer over til
    // resolvedIngredientId. newIngredient er satt ved Approved (opprettes i samme transaksjon) og null ved Merged.
    Task<bool> ResolveAsync(Guid id, UnconfirmedIngredientStatus outcome, Guid resolvedIngredientId,
        Ingredient? newIngredient, DateTimeOffset reviewedAt);
}
