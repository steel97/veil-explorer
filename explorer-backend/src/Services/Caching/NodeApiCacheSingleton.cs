using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;
using ExplorerBackend.Configs;
using MemoryCache = Microsoft.Extensions.Caching.Memory.MemoryCache;

namespace ExplorerBackend.Services.Caching;

public class NodeApiCacheSingleton
{
    private readonly IOptionsMonitor<MemoryCacheConfig> _memoryCacheConfig;
    private MemoryCache Cache { get; set; }
    private readonly List<string> ApisInQueue = new();
    private readonly SemaphoreSlim ApisQueueSemaphore = new(1, 1);

    public NodeApiCacheSingleton(IOptionsMonitor<MemoryCacheConfig> memoryCacheConfig)
    {
        _memoryCacheConfig = memoryCacheConfig;
        Cache = new MemoryCache(new MemoryCacheOptions
        {
            ExpirationScanFrequency = TimeSpan.FromMilliseconds(_memoryCacheConfig.CurrentValue.ExpirationScanFrequency)
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
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(_memoryCacheConfig.CurrentValue.ExpirationApiAbsoluteTime)
        });
    }

    public T? GetApiCache<T>(string key)
    {
        if (!Cache.TryGetValue<T>(key, out var res))
            return default;
        return res;
    }
}