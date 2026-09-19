using Domain.Ingredients;
using Microsoft.Extensions.Configuration;
using Persistence.Services;

namespace Persistence.Implementation;

public class SearchKeywordReader(IConfiguration configuration)
    : DbReader<SearchKeyword>(configuration.GetConnectionString("DefaultConnection")!)
{
    public override string? GetAllQuery { get; } = "SELECT * FROM get_all_search_keyword();";
    public override string? GetByIdQuery { get; } = "SELECT * FROM get_search_keyword_by_id(@Id);";
}
