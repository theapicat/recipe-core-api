using Domain.Ingredients;
using Microsoft.Extensions.Configuration;
using Persistence.Services;

namespace Persistence.Implementation;

public class AllergenReader(IConfiguration configuration)
    : DbReader<Allergen>(configuration.GetConnectionString("DefaultConnection")!)
{
    public override string? GetAllQuery { get; } = "SELECT * FROM get_all_allergen();";
    public override string? GetByIdQuery { get; } = "SELECT * FROM get_allergen_by_id(@Id);";
}
