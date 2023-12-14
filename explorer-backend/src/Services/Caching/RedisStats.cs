using StackExchange.Redis;

namespace ExplorerBackend.Services.Caching;

public class RedisStats
{
    public long MemoryUsageBytes { get; set; }
    public Dictionary<string, RedisResult>? RedisMemoryInfo { get; set; }
}