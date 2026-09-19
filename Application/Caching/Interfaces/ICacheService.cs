namespace Application.Caching.Interfaces;

/// <summary>
/// Rene cache-primitiver - ingen kjennskap til hvor data kommer fra ved cache-miss.
/// Den avgjørelsen (hente fra database, evt. andre kilder) ligger hos den som kaller cachen.
/// </summary>
public interface ICacheService
{
    T? Get<T>(string key);

    void Set<T>(string key, T value, TimeSpan? expiration = null);

    void Remove(string key);
}
