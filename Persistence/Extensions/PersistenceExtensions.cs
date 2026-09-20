using Dapper;
using Domain.Ingredients;
using Domain.Recipes;
using Domain.Units;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Implementation;
using Persistence.Interfaces;
using Persistence.Services;

namespace Persistence.Extensions;

public static class PersistenceExtensions
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services)
    {
        // Postgres-kolonner er snake_case, C#-egenskaper er PascalCase - dette lar Dapper matche dem
        // automatisk (owner_user_id -> OwnerUserId) uten kolonne-alias i hver spørring.
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        SqlMapper.AddTypeHandler(new DateTimeOffsetTypeHandler());

        services.AddScoped<DbReader<IngredientCategory>, IngredientCategoryReader>();
        services.AddScoped<DbWriter<IngredientCategory>, IngredientCategoryWriter>();

        services.AddScoped<DbReader<Allergen>, AllergenReader>();
        services.AddScoped<DbWriter<Allergen>, AllergenWriter>();

        services.AddScoped<DbReader<SearchKeyword>, SearchKeywordReader>();
        services.AddScoped<DbWriter<SearchKeyword>, SearchKeywordWriter>();

        services.AddScoped<DbReader<UnitType>, UnitTypeReader>();
        services.AddScoped<DbWriter<UnitType>, UnitTypeWriter>();

        services.AddScoped<DbReader<Unit>, UnitReader>();
        services.AddScoped<DbWriter<Unit>, UnitWriter>();

        services.AddScoped<DbReader<RecipeCategory>, RecipeCategoryReader>();
        services.AddScoped<DbWriter<RecipeCategory>, RecipeCategoryWriter>();

        // Næringsstoffer er skrivebeskyttet (kun seed-data): bare Reader, ingen Writer.
        services.AddScoped<DbReader<NutrientDefinition>, NutrientDefinitionReader>();

        services.AddScoped<DbReader<IngredientListItem>, IngredientListItemReader>();
        services.AddScoped<DbReader<Ingredient>, IngredientReader>();
        services.AddScoped<DbWriter<Ingredient>, IngredientWriter>();

        services.AddScoped<IUnconfirmedIngredientReader, UnconfirmedIngredientReader>();
        services.AddScoped<IUnconfirmedIngredientWriter, UnconfirmedIngredientWriter>();

        return services;
    }
}
