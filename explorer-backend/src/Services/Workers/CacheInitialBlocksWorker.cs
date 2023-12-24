using Microsoft.Extensions.Options;
using ExplorerBackend.Configs;
using ExplorerBackend.Services.Caching;
using ExplorerBackend.Services.Core;

namespace ExplorerBackend.Services.Workers;

public class CacheInitialBlocksWorker : BackgroundService
{
    private uint _blockHeight;
    private readonly uint _blockToCache;
    private readonly int _blockPullDelay;
    private readonly ILogger<CacheInitialBlocksWorker> _logger;
    private readonly IOptionsMonitor<MemoryCacheConfig> _memoryCacheConfig;
    private readonly IOptionsMonitor<ExplorerConfig> _explorerConfig;
    private readonly SimplifiedBlocksCacheSingleton _smpBlocksCacheSingleton;
    private readonly NodeRequester _nodeRequester;

    public CacheInitialBlocksWorker(ILogger<CacheInitialBlocksWorker> logger, IOptionsMonitor<MemoryCacheConfig> memoryCacheConfig, IOptionsMonitor<ExplorerConfig> explorerConfig,
    SimplifiedBlocksCacheSingleton smpBlocksCacheSingleton, NodeRequester nodeRequester)
    {
        _logger = logger;
        _memoryCacheConfig = memoryCacheConfig;
        _explorerConfig = explorerConfig;
        _smpBlocksCacheSingleton = smpBlocksCacheSingleton;
        _nodeRequester = nodeRequester;
        _blockHeight = 1;

        ArgumentNullException.ThrowIfNull(_memoryCacheConfig.CurrentValue.OldestSimplifiedBlocksCacheCount);
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.PullBlocksDelay);

        _blockToCache = (uint)_memoryCacheConfig.CurrentValue.OldestSimplifiedBlocksCacheCount;
        _blockPullDelay = _explorerConfig.CurrentValue.PullBlocksDelay;

    }
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while(_blockHeight <= _blockToCache)
        {
            try
            {
                var blockHash = await _nodeRequester.GetBlockHash(_blockHeight, cancellationToken);
                if (blockHash is null || blockHash.Result is null)
                    continue;

                var block = await _nodeRequester.GetBlock(blockHash.Result, cancellationToken);
                if(block is null || block.Result is null)
                    continue;

                _smpBlocksCacheSingleton.SetBlockCache(block.Result);
            }
            catch
            {
                _blockHeight--;
                _logger.LogWarning("{serivce} failed to pull block", nameof(CacheInitialBlocksWorker));
            }

            _blockHeight++;

            await Task.Delay(_blockPullDelay, cancellationToken);
        }
    }
}