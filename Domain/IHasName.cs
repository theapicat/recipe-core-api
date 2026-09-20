namespace Domain;

// Felles kontrakt for katalogentiteter med et navn som lagres normalisert (små bokstaver), slik at generisk kode
// kan normalisere det før skriving uten å kjenne den konkrete modellen.
public interface IHasName
{
    string Name { get; set; }
}
