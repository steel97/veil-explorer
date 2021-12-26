using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;
using ExplorerBackend.Configs;
using MemoryCache = Microsoft.Extensions.Caching.Memory.MemoryCache;

namespace ExplorerBackend.Services.Caching;

public class NodeApiCacheSingleton
{
    private readonly IOptionsMonitor<ExplorerConfig> _explorerConfig;
    private MemoryCache Cache { get; set; }

    public NodeApiCacheSingleton(IOptionsMonitor<ExplorerConfig> explorerConfig)
    {
        _explorerConfig = explorerConfig;
        Cache = new MemoryCache(new MemoryCacheOptions
        {
            ExpirationScanFrequency = TimeSpan.FromMilliseconds(_explorerConfig.CurrentValue.MemoryCache?.ExpirationScanFrequency ?? 1000)
        });
    }

    public void SetApiCache(string key, object apiResult)
    {
        Cache.Set(key, apiResult, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(_explorerConfig.CurrentValue.MemoryCache?.ExpirationApiAbsoluteTime ?? 60000)
        });
    }

    public T? GetApiCache<T>(string key)
    {
        T res;
        if (!Cache.TryGetValue<T>(key, out res))
            return default(T);
        return res;
    }
}