namespace Domain.Ingredients;

// Livssyklus for en ubekreftet ingrediens. Rejected er terminal. Approved/Merged betyr at brukeren ikke lenger
// "eier" ingrediensen - den er (eller er koblet til) en offisiell ingrediens tilgjengelig for alle.
public enum UnconfirmedIngredientStatus
{
    // Privat for brukeren, ingen forespørsel sendt til admin.
    NotRequested,

    // Brukeren har bedt om at admin tar den inn i den offisielle katalogen.
    Pending,

    // Admin har opprettet en ny offisiell ingrediens ut av den (evt. som variant av en annen).
    Approved,

    // Admin har funnet at den er en duplikat av en eksisterende ingrediens og koblet oppskriftene dit.
    Merged,

    // Admin har avslått forespørselen (med valgfri begrunnelse). Den forblir en privat ingrediens hos brukeren.
    Rejected
}
