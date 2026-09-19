using Application.Caching.Services;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace Tests.Application.Caching;

public class MemoryCacheServiceTests
{
    private static MemoryCacheService CreateService() => new(new MemoryCache(new MemoryCacheOptions()));

    [Fact]
    public void Get_ReturnsDefault_WhenKeyNotSet()
    {
        var cache = CreateService();

        var result = cache.Get<string>("missing");

        Assert.Null(result);
    }

    [Fact]
    public void Get_ReturnsValue_AfterSet()
    {
        var cache = CreateService();

        cache.Set("key", "value");

        Assert.Equal("value", cache.Get<string>("key"));
    }

    [Fact]
    public void Get_ReturnsDefault_AfterRemove()
    {
        var cache = CreateService();
        cache.Set("key", "value");

        cache.Remove("key");

        Assert.Null(cache.Get<string>("key"));
    }
}
