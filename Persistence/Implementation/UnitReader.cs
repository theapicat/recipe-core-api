using Domain.Units;
using Microsoft.Extensions.Configuration;
using Persistence.Services;

namespace Persistence.Implementation;

public class UnitReader(IConfiguration configuration)
    : DbReader<Unit>(configuration.GetConnectionString("DefaultConnection")!)
{
    public override string? GetAllQuery { get; } = "SELECT * FROM get_all_unit();";
    public override string? GetByIdQuery { get; } = "SELECT * FROM get_unit_by_id(@Id);";
}
