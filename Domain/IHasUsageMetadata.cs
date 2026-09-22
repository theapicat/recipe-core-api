namespace Domain;

// Felles kontrakt for adminstyrte katalogentiteter, slik at generisk kode (DeleteCatalogCommandHandler) kan nekte
// sletting av seed-rader (IsSystem) eller rader som fortsatt er i bruk (UsageCount), uten å kjenne den konkrete
// modellen. Begge er serverstyrte og skrivebeskyttet: IsSystem settes kun av seed-data, UsageCount beregnes ved
// lesing - verken insert_<catalog> eller update_<catalog> refererer @IsSystem/@UsageCount, så Dapper ignorerer dem
// uansett hva klienten sender (samme triks som Ingredient.IsOfficial).
public interface IHasUsageMetadata
{
    bool IsSystem { get; set; }
    int UsageCount { get; set; }
}
