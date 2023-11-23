using ExplorerBackend.Configs;
using ExplorerBackend.Models.Data;
using Microsoft.Extensions.Options;
using System.Text.Json;
using StackExchange.Redis;
using ExplorerBackend.Services.Data;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using ExplorerBackend.Models.Node.Response;

namespace ExplorerBackend.Services.Caching;

public class BlocksCacheSingleton
{
    public GetBlockResult? LatestBlock {get; private set;}
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

        ArgumentNullException.ThrowIfNull(_memoryCacheConfig.CurrentValue.ServerAbsoluteExpirationCacheTimeMin);
        ArgumentNullException.ThrowIfNull(_memoryCacheConfig.CurrentValue.UserAbsoluteExpirationCacheTimeSec);

        _serverAbsExpTime = TimeSpan.FromMinutes(_memoryCacheConfig.CurrentValue.ServerAbsoluteExpirationCacheTimeMin);
        _userAbsExpTime = TimeSpan.FromMinutes(_memoryCacheConfig.CurrentValue.UserAbsoluteExpirationCacheTimeSec);
    }
    // TODO: implement MemoryPack to deal with byte[], create redis key-value templ (height -> hash, height:s - simplified block), 
    //       create logic for saving simplified blocks (optionaly batch N per 1 request instead of N requests)
    public async Task<bool> UserCacheDataAsync(int blockHeight, string blockHash, GetBlockResult blockData, CancellationToken ct = default)
    {
        var redisServer = _cache.GetServer();
        RedisResult redisResult =  await redisServer.ExecuteAsync("DBSIZE");
        double.TryParse(redisResult.ToString(), out double totalCachedObj);

        if(totalCachedObj < )
        {
            var redis = _cache.GetDatabase();
            // will be MemoryPack
            string userJsonData = JsonSerializer.Serialize(blockData);
            Task userKeyDatapair = redis.StringSetAsync(blockHeight.ToString(), blockHash, _userAbsExpTime);
            Task userHashKeyPair = redis.StringSetAsync(blockHash, userJsonData, _userAbsExpTime);
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

        string serverData = JsonSerializer.Serialize(blockData);

        Task keyDataPair = _cache.SetAsync(blockHash, serverData, _serverOptions, ct);
        Task hashKeyPair = _cache.SetAsync(blockHeight, blockHash, _serverOptions, ct);
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
       var a = await _cache.GetAsync(height, ct);
    }
    
    public async Task<GetBlockResult?> GetCachedBlockAsync<GetBlockResult>(string hash, CancellationToken ct)
    {
        var rawBlock = await _cache.GetAsync(hash, ct);
        if(rawBlock is null) return default!;        

        return JsonSerializer.Deserialize<GetBlockResult>(rawBlock);
    }
    
    public async Task<GetBlockResult?> GetCachedBlockByHeightAsync<GetBlockResult>(string height, CancellationToken ct)
    {
        var blockHash = await _cache.GetAsync(height, ct);
        if(blockHash is null) return default;

        var rawBlock = await _cache.GetAsync(blockHash, ct);
        if(rawBlock is null) return default;

        return JsonSerializer.Deserialize<GetBlockResult>(rawBlock);
    }

    public async Task UpdateCachedDataAsync(string height, string newHash, GetBlockResult newData, CancellationToken ct = default)
    {
        var oldHash = await _cache.GetAsync(height, ct);

        Task heightHashPair = _cache.RemoveAsync(height, ct);
        Task hashDataPair = _cache.RemoveAsync(oldHash!, ct);

        try
        {
            await Task.WhenAll(heightHashPair, hashDataPair);
        }
        catch
        {
            _logger.LogError("can't update cache");
        }

        string serverData = JsonSerializer.Serialize(newData);

        Task heightHashNewPair = _cache.SetAsync(height, serverData, _serverOptions, ct);
        Task hashDataNewPair = _cache.SetAsync(newHash, height, _serverOptions, ct);
        try
        {
            await Task.WhenAll(heightHashNewPair, hashDataNewPair);
        }
        catch
        {
            _logger.LogError("can't update cache");
        }
    }

    public async ValueTask<bool> ValidateCacheAsync(string height, string newHash)
    {        
        var cachedHash = await _cache.GetAsync(height);

        return cachedHash != newHash;
    }
}