using ExplorerBackend.Configs;
using Microsoft.Extensions.Options;
using ExplorerBackend.Services.Caching;
using StackExchange.Redis;

namespace ExplorerBackend.Services.Workers;

public class RedisStatWorker : BackgroundService
{

    private readonly string _host;
    private readonly int _port;
    private readonly RedisStats _redisStats;
    private readonly IConnectionMultiplexer _cache;
    private readonly IOptionsMonitor<MemoryCacheConfig> _memoryCacheConfig;
    public RedisStatWorker(RedisStats redisStats, IConnectionMultiplexer cache, IOptionsMonitor<MemoryCacheConfig> memoryCacheConfig)
    {
        _redisStats = redisStats;
        _cache = cache;
        _memoryCacheConfig = memoryCacheConfig;

        ArgumentNullException.ThrowIfNull(_memoryCacheConfig.CurrentValue.Port);
        ArgumentNullException.ThrowIfNull(_memoryCacheConfig.CurrentValue.Host);

        _port = _memoryCacheConfig.CurrentValue.Port;
        _host = _memoryCacheConfig.CurrentValue.Host;
    }
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var redisServer = _cache.GetServer(_host, _port);
                var memoryStats = await redisServer.MemoryStatsAsync();

                if (memoryStats is null) continue;

                _redisStats.RedisMemoryInfo = memoryStats.ToDictionary();

                _redisStats.RedisMemoryInfo.TryGetValue("total.allocated", out RedisResult? memoryUsed);

                if (memoryUsed is null) continue;

                _ = long.TryParse(memoryUsed.ToString(), out long memoryValue);

                _redisStats.MemoryUsageBytes = memoryValue;
            }
            catch { }
        }

        await Task.Delay(5000, cancellationToken);
    }
}