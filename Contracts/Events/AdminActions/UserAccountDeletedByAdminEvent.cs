namespace Contracts.Events.AdminActions;

// Publisert av recipe-auth-api når en admin sletter en annen bruker. Se UserAccountDeletedByUserEvent for
// navnerom-/felt-kravet.
public record UserAccountDeletedByAdminEvent
{
    public required Guid UserId { get; init; }
    public required string Email { get; init; }
    public required string Name { get; init; }
    public required DateTime DeletedAt { get; init; }
}
