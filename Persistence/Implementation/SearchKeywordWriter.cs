using Domain.Ingredients;
using Microsoft.Extensions.Configuration;
using Persistence.Services;

namespace Persistence.Implementation;

public class SearchKeywordWriter(IConfiguration configuration)
    : DbWriter<SearchKeyword>(configuration.GetConnectionString("DefaultConnection")!)
{
    public override string? InsertCommand { get; } = "SELECT insert_search_keyword(@Id, @Name);";
    public override string? UpdateCommand { get; } = "SELECT update_search_keyword(@Id, @Name);";
    public override string? DeleteCommand { get; } = "SELECT delete_search_keyword(@Id);";
}
