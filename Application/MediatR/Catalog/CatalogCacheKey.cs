namespace Application.MediatR.Catalog;

internal static class CatalogCacheKey
{
    public static string ForAll<T>() => $"catalog:{typeof(T).Name}:all";
}
