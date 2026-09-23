namespace Persistence.Interfaces;

// Sletter alt brukerskapt data for én bruker i én transaksjon - kalt når kontoen er slettet i recipe-auth-api (se
// Application/Messaging/Consumers). Utvides etter hvert som flere brukereide feature-tabeller kommer til
// (måltidsplan, handleliste, produkter) - ikke en fast liste å bytte ut, men ett sted å legge til flere slettinger.
public interface IUserDataEraser
{
    Task<UserDataDeletionResult> DeleteAllForUserAsync(Guid userId);
}

public record UserDataDeletionResult(int RecipesDeleted, int UnconfirmedIngredientsDeleted);
