using Microsoft.Extensions.Caching.Memory;
using Products.Application.Interfaces;

namespace Products.Infrastructure.Cache;

public class StatusCache : IStatusCache
{
    private const string CacheKey = "product-status-names";

    private readonly IMemoryCache _cache;
    private readonly StatusCacheOptions _options;

    public StatusCache(IMemoryCache cache, StatusCacheOptions options)
    {
        _cache = cache;
        _options = options;
    }

    public string GetStatusName(int status)
    {
        var statusNames = _cache.GetOrCreate(CacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.TtlMinutes);
            return new Dictionary<int, string>
            {
                { 1, "Active" },
                { 0, "Inactive" }
            };
        })!;

        return statusNames.TryGetValue(status, out var name) ? name : "Unknown";
    }
}
