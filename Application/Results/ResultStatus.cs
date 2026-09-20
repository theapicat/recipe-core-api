namespace Application.Results;

// Forventede forretningsfeil returneres som en status i stedet for å kaste unntak. Kontrolleren oversetter til HTTP.
public enum ResultStatus
{
    Ok,
    NotFound,
    Conflict,
    Invalid
}
