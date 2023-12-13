using Microsoft.Extensions.Options;
using ExplorerBackend.Configs;
using ExplorerBackend.Services.Caching;
using ExplorerBackend.Services.Core;

namespace ExplorerBackend.Services.Workers;

public class CacheInitialBlocks
    : BackgroundService
{
    private uint _blockHeight;
    private readonly uint _blockToCache;
    private readonly int _blockPullDelay;
    private readonly ILogger _logger;
    private readonly IOptionsMonitor<ExplorerConfig> _explorerConfig;
    private readonly SimplifiedBlocksCacheSingleton _smpBlocksCacheSingleton;
    private readonly NodeRequester _nodeRequester;

    public CacheInitialBlocks(ILogger logger, IOptionsMonitor<ExplorerConfig> explorerConfig, BlocksCacheSingleton blocksCacheSingleton,
    SimplifiedBlocksCacheSingleton smpBlocksCacheSingleton, NodeRequester nodeRequester)
    {
        _logger = logger;
        _explorerConfig = explorerConfig;
        _smpBlocksCacheSingleton = smpBlocksCacheSingleton;
        _nodeRequester = nodeRequester;
        _blockHeight = 1;

        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.OldestSimplifiedBlocksCacheCount);
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.PullBlocksDelay);

        _blockToCache = (uint)_explorerConfig.CurrentValue.OldestSimplifiedBlocksCacheCount;
        _blockPullDelay = _explorerConfig.CurrentValue.PullBlocksDelay;

    }
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while(_blockHeight <= _blockToCache)
        {
            try
            {
                var block = await _nodeRequester.GetBlockHash(_blockHeight, cancellationToken);
                if (block is null || block.Result is null)
                    continue;

                var newBlock = await _nodeRequester.GetBlock(block.Result, cancellationToken);
                if(newBlock is null)
                    continue;

                var setSimplifiedBlockCacke = _smpBlocksCacheSingleton.SetBlockCache(newBlock);
            }
            catch
            {
                _logger.LogWarning("CacheInitialBlocks failed to pull block");
            }

            _blockHeight++;

            await Task.Delay(_blockPullDelay, cancellationToken);
        }
    }
}