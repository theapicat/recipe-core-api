namespace Contracts.Events.AdminActions;

// Publisert av recipe-auth-api når en admin sletter og svartelister en bruker. Se UserAccountDeletedByUserEvent for
// navnerom-/felt-kravet.
public record UserDeletedAndBlacklistedByAdminEvent
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Reason { get; init; }
    public DateTime DeletedAt { get; init; }
}
