using Dapper;
using Domain.Ingredients;
using Microsoft.Extensions.Configuration;
using Persistence.Services;

namespace Persistence.Implementation;

// Statisk, skrivebeskyttet katalog. Lesefunksjonene returnerer én flat rad per stoff (enhet og gruppe utfoldet); readeren
// bygger dem om til NutrientDefinition med gruppen nøstet inni.
public class NutrientDefinitionReader(IConfiguration configuration)
    : DbReader<NutrientDefinition>(configuration.GetConnectionString("DefaultConnection")!)
{
    public override string? GetAllQuery { get; } = "SELECT * FROM get_all_nutrient_definition();";
    public override string? GetByIdQuery { get; } = "SELECT * FROM get_nutrient_definition_by_id(@Id);";

    public override async Task<List<NutrientDefinition>> GetAllAsync()
    {
        await using var connection = await OpenConnectionAsync();
        var rows = await connection.QueryAsync<NutrientDefinitionRow>(GetAllQuery!);
        return rows.Select(r => r.ToNutrientDefinition()).ToList();
    }

    public override async Task<NutrientDefinition?> GetByIdAsync<TId>(TId id)
    {
        await using var connection = await OpenConnectionAsync();
        var row = await connection.QuerySingleOrDefaultAsync<NutrientDefinitionRow>(GetByIdQuery!, new { Id = id });
        return row?.ToNutrientDefinition();
    }
}
