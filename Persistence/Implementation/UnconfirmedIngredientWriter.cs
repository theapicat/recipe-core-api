using Dapper;
using Domain.Ingredients;
using Microsoft.Extensions.Configuration;
using Persistence.Interfaces;
using Persistence.Services;

namespace Persistence.Implementation;

public class UnconfirmedIngredientWriter(IConfiguration configuration)
    : DbWriter<UnconfirmedIngredient>(configuration.GetConnectionString("DefaultConnection")!),
        IUnconfirmedIngredientWriter
{
    // Enum sendes som tekst (kolonnen er text med CHECK) - Dapper sender ellers enum som heltall.
    public override async Task AddAsync(UnconfirmedIngredient entity)
    {
        await using var connection = await OpenConnectionAsync();
        await connection.ExecuteAsync(
            "SELECT insert_unconfirmed_ingredient(@Id, @Name, @CreatedByUserId, @ReviewStatus, @CreatedAt);",
            new { entity.Id, entity.Name, entity.CreatedByUserId, ReviewStatus = entity.ReviewStatus.ToString(), entity.CreatedAt });
    }

    public async Task<bool> RenameAsync(Guid id, Guid userId, string name)
    {
        await using var connection = await OpenConnectionAsync();
        var affected = await connection.ExecuteScalarAsync<int>(
            "SELECT update_unconfirmed_ingredient_name(@Id, @UserId, @Name);", new { Id = id, UserId = userId, Name = name });
        return affected > 0;
    }

    public async Task<bool> RequestReviewAsync(Guid id, Guid userId)
    {
        await using var connection = await OpenConnectionAsync();
        var affected = await connection.ExecuteScalarAsync<int>(
            "SELECT request_unconfirmed_ingredient_review(@Id, @UserId);", new { Id = id, UserId = userId });
        return affected > 0;
    }

    public async Task<bool> DeleteOwnAsync(Guid id, Guid userId)
    {
        await using var connection = await OpenConnectionAsync();
        var affected = await connection.ExecuteScalarAsync<int>(
            "SELECT delete_unconfirmed_ingredient(@Id, @UserId);", new { Id = id, UserId = userId });
        return affected > 0;
    }

    public async Task<bool> RejectAsync(Guid id, string? reason, DateTimeOffset reviewedAt)
    {
        await using var connection = await OpenConnectionAsync();
        var affected = await connection.ExecuteScalarAsync<int>(
            "SELECT reject_unconfirmed_ingredient(@Id, @Reason, @ReviewedAt);", new { Id = id, Reason = reason, ReviewedAt = reviewedAt });
        return affected > 0;
    }

    public async Task<bool> ResolveAsync(Guid id, UnconfirmedIngredientStatus outcome, Guid resolvedIngredientId,
        Ingredient? newIngredient, DateTimeOffset reviewedAt)
    {
        await using var connection = await OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        if (newIngredient is not null)
            await IngredientPersistence.InsertAsync(connection, transaction, newIngredient);

        var affected = await connection.ExecuteScalarAsync<int>(
            "SELECT resolve_unconfirmed_ingredient(@Id, @Status, @ResolvedIngredientId, @ReviewedAt);",
            new { Id = id, Status = outcome.ToString(), ResolvedIngredientId = resolvedIngredientId, ReviewedAt = reviewedAt },
            transaction);

        // Ikke lenger ventende (eller finnes ikke): rull tilbake, inkl. en evt. nettopp opprettet ingrediens.
        if (affected == 0)
        {
            await transaction.RollbackAsync();
            return false;
        }

        await transaction.CommitAsync();
        return true;
    }
}
