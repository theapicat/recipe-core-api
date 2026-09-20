namespace Application.MediatR.User.UnconfirmedIngredients;

// Grenser mot misbruk. Ingen bruker trenger i nærheten av så mange egne ingredienser; den strengere grensen
// for ventende forespørsler hindrer at admin-køen oversvømmes.
public static class UnconfirmedIngredientLimits
{
    public const int MaxPerUser = 100;
    public const int MaxPendingPerUser = 10;
    public const int MaxNameLength = 200;
}
