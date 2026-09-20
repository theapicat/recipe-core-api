using Domain.Ingredients;

namespace Persistence.Interfaces;

// Egen kontrakt fordi ubekreftede ingredienser trenger spørringer utover malen i DbReader<T> (per bruker,
// per status, telling til grenser).
public interface IUnconfirmedIngredientReader
{
    Task<UnconfirmedIngredient?> GetByIdAsync(Guid id);

    Task<List<UnconfirmedIngredient>> GetByUserAsync(Guid userId);

    // status = null gir alle (admin-køen filtrerer vanligvis på Pending).
    Task<List<UnconfirmedIngredient>> GetByStatusAsync(UnconfirmedIngredientStatus? status);

    Task<int> CountByUserAsync(Guid userId, UnconfirmedIngredientStatus? status = null);
}
