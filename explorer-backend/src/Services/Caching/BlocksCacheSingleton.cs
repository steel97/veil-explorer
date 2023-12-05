using ExplorerBackend.Configs;
using Microsoft.Extensions.Options;
using System.Text.Json;
using StackExchange.Redis;
using ExplorerBackend.Models.Node.Response;

namespace ExplorerBackend.Services.Caching;

public class BlocksCacheSingleton
{
    public GetBlockResult? LatestBlock {get; private set;}
    private readonly string _host;
    private readonly int _port;
    private readonly TimeSpan _serverAbsExpTime;
    private readonly TimeSpan _userAbsExpTime;
    private readonly ILogger _logger;
    private readonly IConnectionMultiplexer _cache;
    private readonly IOptionsMonitor<MemoryCache> _memoryCacheConfig;
    public BlocksCacheSingleton(ILogger logger, IConnectionMultiplexer cache, IOptionsMonitor<MemoryCache> memoryCacheConfig)
    {
        _logger = logger;
        _memoryCacheConfig = memoryCacheConfig;
        _cache = cache;

        ArgumentNullException.ThrowIfNull(_memoryCacheConfig.CurrentValue.Port);
        ArgumentNullException.ThrowIfNull(_memoryCacheConfig.CurrentValue.Host);
        ArgumentNullException.ThrowIfNull(_memoryCacheConfig.CurrentValue.ServerAbsExpCacheTimeDays);
        ArgumentNullException.ThrowIfNull(_memoryCacheConfig.CurrentValue.UserAbsExpCacheTimeSec);

        _port = _memoryCacheConfig.CurrentValue.Port;
        _host = _memoryCacheConfig.CurrentValue.Host;
        _serverAbsExpTime = TimeSpan.FromMinutes(_memoryCacheConfig.CurrentValue.ServerAbsExpCacheTimeDays);
        _userAbsExpTime = TimeSpan.FromMinutes(_memoryCacheConfig.CurrentValue.UserAbsExpCacheTimeSec);
    }
    // TODO: implement MemoryPack to deal with byte[], create redis key-value templ (height -> hash, height:s - simplified block)
    public async Task<bool> UserCacheDataAsync(int blockHeight, string blockHash, GetBlockResult blockData, CancellationToken ct = default)
    {
        var redisServer = _cache.GetServer(_host, _port);
        RedisResult redisResult =  await redisServer.ExecuteAsync("DBSIZE");
        bool parseResult = double.TryParse(redisResult.ToString(), out double dbSize);
        // MB or bytes?
        if(dbSize <= 600 && parseResult)
        {
            var redis = _cache.GetDatabase();

            Task userKeyDatapair = redis.StringSetAsync(blockHeight.ToString(), blockHash, _userAbsExpTime);
            Task userHashKeyPair = redis.StringSetAsync(blockHash, JsonSerializer.Serialize(blockData), _userAbsExpTime);
            try
            {
                await Task.WhenAll(userKeyDatapair, userHashKeyPair);
            }
            catch
            {
                _logger.LogError("can't set user cache");
                return false;
            }
            return true;
        }
        return false;
    }

    public async Task<bool> SetServerCacheDataAsync(int blockHeight, string blockHash, GetBlockResult blockData, CancellationToken ct = default)
    {
        LatestBlock = blockData;
        var db = _cache.GetDatabase();

        Task hashKeyPair = db.StringSetAsync(blockHeight.ToString(), blockHash,_serverAbsExpTime);
        Task keyDataPair = db.StringSetAsync(blockHash, JsonSerializer.Serialize(blockData), _serverAbsExpTime);
        try
        {
            await Task.WhenAll(keyDataPair, hashKeyPair);
        }
        catch
        {
            _logger.LogError("can't set cache");
            return false;
        }
        return true;
    }

    public async Task<string?> GetCachedBlockHashAsync(int height, CancellationToken ct)
    {
       var db = _cache.GetDatabase();
       return await db.StringGetAsync(height.ToString());
    }
    
    public async Task<GetBlockResult?> GetCachedBlockAsync<GetBlockResult>(string hash, CancellationToken ct)
    {
        var db = _cache.GetDatabase();
        RedisValue? rawBlock = await db.StringGetAsync(hash);
        if(rawBlock is null) return default;

        return JsonSerializer.Deserialize<GetBlockResult>(rawBlock!);
    }
    
    public async Task<GetBlockResult?> GetCachedBlockByHeightAsync<GetBlockResult>(string height, CancellationToken ct)
    {
        var db = _cache.GetDatabase();
        RedisValue? blockHash = await db.StringGetAsync(height);
        if(blockHash is null) return default;

        string? hash = blockHash.Value;
        RedisValue? rawBlock = await db.StringGetAsync(hash);
        if(rawBlock is null) return default;

        return JsonSerializer.Deserialize<GetBlockResult>(rawBlock!);
    }

    public async Task UpdateCachedDataAsync(string height, string newHash, GetBlockResult newData, CancellationToken ct = default)
    {
        var db = _cache.GetDatabase();
        RedisValue? oldHash = await db.StringGetAsync(height);
        string? oldHashStr = oldHash.Value;
        Task heightHashPair = db.StringSetAsync(height, newHash, _serverAbsExpTime);
        Task hashDataPairDeletion = db.StringSetAsync(oldHashStr, string.Empty, TimeSpan.FromSeconds(1));

        Task hashDataPair = db.StringSetAsync(newHash, JsonSerializer.Serialize(newData), _serverAbsExpTime);

        try
        {
            await Task.WhenAll(heightHashPair, hashDataPairDeletion, hashDataPair);
        }
        catch
        {
            _logger.LogError("can't update cache");
        }
    }

    public async ValueTask<bool> ValidateCacheAsync(string height, string newHash)
    {     
        var db = _cache.GetDatabase();   
        var cachedHash = await db.StringGetAsync(height);

        return cachedHash == newHash;
    }
}