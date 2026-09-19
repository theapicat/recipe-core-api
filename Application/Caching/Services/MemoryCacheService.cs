using Application.Caching.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Caching.Services;

public class MemoryCacheService(IMemoryCache cache) : ICacheService
{
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(30);

    public T? Get<T>(string key) => cache.TryGetValue(key, out T? value) ? value : default;

    public void Set<T>(string key, T value, TimeSpan? expiration = null)
        => cache.Set(key, value, expiration ?? DefaultExpiration);

    public void Remove(string key) => cache.Remove(key);
}
