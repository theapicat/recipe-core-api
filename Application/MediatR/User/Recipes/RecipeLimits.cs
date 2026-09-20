namespace Application.MediatR.User.Recipes;

// Grenser mot misbruk og for å holde lista håndterbar (hele oppskriftslista lastes til klienten). MaxPerUser er samlet her slik at
// den enkelt kan gjøres avhengig av kontotype senere (f.eks. en betalt konto med høyere grense).
public static class RecipeLimits
{
    public const int MaxPerUser = 500;
    public const int MaxTitleLength = 200;
    public const int MaxDescriptionLength = 5000;
    public const int MaxStepLength = 2000;
    public const int MaxNoteLength = 200;
    public const int MaxReferenceLength = 200;
    public const int MaxImageUrlLength = 2000;
    public const int MaxImageAttributionLength = 200;
    public const int MaxSteps = 100;
    public const int MaxIngredients = 100;
    public const int MaxServings = 1000;

    // En uke - en øvre grense som fanger åpenbare tastefeil, ikke en faglig regel.
    public const int MaxTimerMinutes = 10080;

    // numeric(10,3) i databasen.
    public const decimal MaxAmount = 9_999_999m;
}
