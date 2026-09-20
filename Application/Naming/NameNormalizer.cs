using System.Text.RegularExpressions;

namespace Application.Naming;

// Alle navn (kataloger, ingredienser, søkeord, titler) lagres med små bokstaver og enkle mellomrom. Frontend gjør om
// til stor forbokstav ved visning. Samme funksjon brukes ved skriving og ved søk, så sammenligning alltid er lik.
public static partial class NameNormalizer
{
    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    public static string Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : Whitespace().Replace(value.Trim(), " ").ToLowerInvariant();
}
