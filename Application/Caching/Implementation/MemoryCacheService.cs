using Application.Caching.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Caching.Implementation;

public class MemoryCacheService(IMemoryCache cache) : ICacheService
{
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(30);

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        if (cache.TryGetValue(key, out T? cached) && cached is not null)
            return cached;

        var value = await factory();
        cache.Set(key, value, expiration ?? DefaultExpiration);
        return value;
    }

    public void Remove(string key) => cache.Remove(key);
}
