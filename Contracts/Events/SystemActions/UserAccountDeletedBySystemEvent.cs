namespace Contracts.Events.SystemActions;

// Publisert av recipe-auth-api sin Quartz-jobb (30 dager ubekreftet e-post, eller 1 år inaktivitet + 30 dagers
// utestengelse). Se UserAccountDeletedByUserEvent for navnerom-/felt-kravet.
public record UserAccountDeletedBySystemEvent
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string DeletionReason { get; init; } = string.Empty;
    public DateTime DeletedAt { get; init; }
}
