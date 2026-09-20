namespace Domain;

// Felles kontrakt for entiteter med en primærnøkkel, slik at generisk kode (katalog-CQRS og
// -kontrollere) kan lese og - for Guid-nøkler - tildele id uten å kjenne den konkrete modellen.
public interface IHasId<TKey>
{
    TKey Id { get; set; }
}
