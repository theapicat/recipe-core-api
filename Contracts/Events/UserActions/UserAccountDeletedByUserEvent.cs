namespace Contracts.Events.UserActions;

// Publisert av recipe-auth-api når en bruker sletter sin egen konto. Navn/navnerom/felt må være byte-for-byte likt
// på tvers av tjenestene - MassTransits standardtopologi binder utveksling til meldingstypens fullt kvalifiserte navn.
public record UserAccountDeletedByUserEvent
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public DateTime DeletedAt { get; init; }
}
