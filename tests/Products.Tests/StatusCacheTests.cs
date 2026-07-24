using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Products.Infrastructure.Cache;

namespace Products.Tests;

public class StatusCacheTests
{
    private static StatusCache CreateCache()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        return new StatusCache(memoryCache, new StatusCacheOptions { TtlMinutes = 5 });
    }

    [Theory]
    [InlineData(1, "Active")]
    [InlineData(0, "Inactive")]
    public void GetStatusName_ReturnsExpectedName(int status, string expected)
    {
        var cache = CreateCache();

        var name = cache.GetStatusName(status);

        name.Should().Be(expected);
    }

    [Fact]
    public void GetStatusName_CalledTwice_ReturnsSameValue()
    {
        var cache = CreateCache();

        var first = cache.GetStatusName(1);
        var second = cache.GetStatusName(1);

        first.Should().Be("Active");
        second.Should().Be(first);
    }

    [Fact]
    public void GetStatusName_WithUnknownStatus_ReturnsUnknown()
    {
        var cache = CreateCache();

        var name = cache.GetStatusName(99);

        name.Should().Be("Unknown");
    }
}
