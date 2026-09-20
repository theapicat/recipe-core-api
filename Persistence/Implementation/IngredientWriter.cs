using Domain.Ingredients;
using Microsoft.Extensions.Configuration;
using Persistence.Services;

namespace Persistence.Implementation;

// Overstyrer opprett/oppdater slik at ingrediensen og alle barna skrives i én transaksjon (malen i
// DbWriter<T> åpner ellers en ny forbindelse per kall). Sletting bruker malens standard DeleteCommand.
public class IngredientWriter(IConfiguration configuration)
    : DbWriter<Ingredient>(configuration.GetConnectionString("DefaultConnection")!)
{
    public override string? DeleteCommand { get; } = "SELECT delete_ingredient(@Id);";

    public override async Task AddAsync(Ingredient entity)
    {
        await using var connection = await OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await IngredientPersistence.InsertAsync(connection, transaction, entity);
        await transaction.CommitAsync();
    }

    public override async Task UpdateAsync(Ingredient entity)
    {
        await using var connection = await OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await IngredientPersistence.UpdateAsync(connection, transaction, entity);
        await transaction.CommitAsync();
    }
}
