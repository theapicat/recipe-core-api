using Domain.Ingredients;
using Microsoft.Extensions.Configuration;
using Persistence.Services;

namespace Persistence.Implementation;

public class NutrientDefinitionReader(IConfiguration configuration)
    : DbReader<NutrientDefinition>(configuration.GetConnectionString("DefaultConnection")!)
{
    public override string? GetAllQuery { get; } = "SELECT * FROM get_all_nutrient_definition();";
    public override string? GetByIdQuery { get; } = "SELECT * FROM get_nutrient_definition_by_id(@Id);";
}
