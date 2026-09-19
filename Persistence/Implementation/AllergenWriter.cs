using Domain.Ingredients;
using Microsoft.Extensions.Configuration;
using Persistence.Services;

namespace Persistence.Implementation;

public class AllergenWriter(IConfiguration configuration)
    : DbWriter<Allergen>(configuration.GetConnectionString("DefaultConnection")!)
{
    public override string? InsertCommand { get; } = "SELECT insert_allergen(@Id, @Name);";
    public override string? UpdateCommand { get; } = "SELECT update_allergen(@Id, @Name);";
    public override string? DeleteCommand { get; } = "SELECT delete_allergen(@Id);";
}
