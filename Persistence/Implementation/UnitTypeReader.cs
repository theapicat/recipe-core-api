using Domain.Units;
using Microsoft.Extensions.Configuration;
using Persistence.Services;

namespace Persistence.Implementation;

public class UnitTypeReader(IConfiguration configuration)
    : DbReader<UnitType>(configuration.GetConnectionString("DefaultConnection")!)
{
    public override string? GetAllQuery { get; } = "SELECT * FROM get_all_unit_type();";
    public override string? GetByIdQuery { get; } = "SELECT * FROM get_unit_type_by_id(@Id);";
}
