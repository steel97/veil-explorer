using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;
using ExplorerBackend.Configs;
using MemoryCache = Microsoft.Extensions.Caching.Memory.MemoryCache;

namespace ExplorerBackend.Services.Caching;

public class NodeApiCacheSingleton
{
    private readonly IOptionsMonitor<ExplorerConfig> _explorerConfig;
    private MemoryCache Cache { get; set; }
    private readonly List<string> ApisInQueue = new();
    private readonly SemaphoreSlim ApisQueueSemaphore = new(1, 1);

    public NodeApiCacheSingleton(IOptionsMonitor<ExplorerConfig> explorerConfig)
    {
        _explorerConfig = explorerConfig;
        Cache = new MemoryCache(new MemoryCacheOptions
        {
            ExpirationScanFrequency = TimeSpan.FromMilliseconds(_explorerConfig.CurrentValue.MemoryCache?.ExpirationScanFrequency ?? 1000)
        });
    }

    public async Task<bool> PutInQueueAsync(string key)
    {
        var res = false;
        try
        {
            await ApisQueueSemaphore.WaitAsync();

            try
            {
                if (!ApisInQueue.Contains(key))
                {
                    ApisInQueue.Add(key);
                    res = true;
                }
            }
            catch
            {

            }

            ApisQueueSemaphore.Release();
        }
        catch
        {

        }
        return res;
    }

    public async Task RemoveFromQueueAsync(string key)
    {
        try
        {
            await ApisQueueSemaphore.WaitAsync();

            try
            {
                if (ApisInQueue.Contains(key))
                    ApisInQueue.Remove(key);
            }
            catch
            {

            }

            ApisQueueSemaphore.Release();
        }
        catch
        {

        }
    }

    public bool IsInQueue(string key) => ApisInQueue.Contains(key);

    public void SetApiCache(string key, object apiResult)
    {
        Cache.Set(key, apiResult, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(_explorerConfig.CurrentValue.MemoryCache?.ExpirationApiAbsoluteTime ?? 60000)
        });
    }

    public T? GetApiCache<T>(string key)
    {
        if (!Cache.TryGetValue<T>(key, out var res))
            return default;
        return res;
    }
}