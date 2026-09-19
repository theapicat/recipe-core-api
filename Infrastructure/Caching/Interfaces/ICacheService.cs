namespace Infrastructure.Caching.Interfaces;

/// <summary>
/// Cache for data som sjelden endres, f.eks. admin-kataloger (kategorier, allergener, enheter).
/// </summary>
public interface ICacheService
{
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);

    void Remove(string key);
}
