using ExplorerBackend.Models.API;
using ExplorerBackend.Models.Data;
using ExplorerBackend.Configs;
using ExplorerBackend.Services.Caching;
using Microsoft.Extensions.Options;
using ExplorerBackend.Models.Node.Response;
using ExplorerBackend.Services.Core;

namespace ExplorerBackend.Services.Data;

public class RealtimeBlocksDataService : IBlocksDataService
{
    private readonly ILogger<RealtimeBlocksDataService> _logger;
    private readonly NodeRequester _nodeRequester;
    private readonly BlocksCacheSingleton _cache;
    private readonly SimplifiedBlocksCacheSingleton _smpBlocksCache;
    private readonly ChaininfoSingleton _chaininfoSingleton;
    private readonly IBlocksService _blocksService;
    public RealtimeBlocksDataService(ILogger<RealtimeBlocksDataService> logger, BlocksCacheSingleton cache, SimplifiedBlocksCacheSingleton smpBlocksCache,
        NodeRequester nodeRequester, ChaininfoSingleton chaininfoSingleton, IBlocksService blocksService) =>
        (_logger, _cache, _smpBlocksCache, _nodeRequester, _chaininfoSingleton, _blocksService) = 
        (logger, cache, smpBlocksCache, nodeRequester, chaininfoSingleton, blocksService);
    
    public async Task<Block?> GetBlockAsync(string hash, int simplifiedTxInfo = 2, CancellationToken cancellationToken = default)
    {
        GetBlock? rawBlock = await _cache.GetCachedBlockAsync<GetBlock>(hash, cancellationToken);        
        rawBlock ??= await _nodeRequester.GetBlock(hash, cancellationToken, simplifiedTxInfo);

        if(rawBlock is null || rawBlock.Result is null)
            return default;

        return _blocksService.RPCBlockToDb(rawBlock.Result);
    }

    public async Task<Block?> GetBlockAsync(int height, int simplifiedTxInfo = 2, CancellationToken cancellationToken = default)
    {
        GetBlock? rawBlock = await _cache.GetCachedBlockByHeightAsync<GetBlock>(height.ToString(), cancellationToken);

        if(rawBlock is null)
        {
            var blockHash = await _nodeRequester.GetBlockHash((uint)height, cancellationToken);
            rawBlock = await _nodeRequester.GetBlock(blockHash!.Result!, cancellationToken, simplifiedTxInfo);
            
            if(rawBlock is not null)
                await _cache.SetUserCacheDataAsync(height, rawBlock.Result!.Hash!, rawBlock.Result, cancellationToken);
        }

        if(rawBlock is null || rawBlock.Result is null)
            return default;

        return _blocksService.RPCBlockToDb(rawBlock.Result);
    }

    public async Task<Block?> GetLatestBlockAsync(bool onlySynced = true, CancellationToken cancellationToken = default)
    {
        if(_cache.LatestBlock is null)
        {
            var rawBlock = await _nodeRequester.GetLatestBlock(cancellationToken);
            return _blocksService.RPCBlockToDb(rawBlock!.Result!);   
        }
        return _blocksService.RPCBlockToDb(_cache.LatestBlock!);
    }

    public async Task<List<SimplifiedBlock>> GetSimplifiedBlocksAsync(int offset, int count, SortDirection sort = SortDirection.DESC, CancellationToken ct = default)
    {
        // get from cache or rpc (it's may crash node lol)💀💀
        List<SimplifiedBlock> blocksList = new(count);
        List<Task<GetBlock>> rawBlocksList = new(count);
        
        int height;

        if (sort == SortDirection.DESC)
            height = _chaininfoSingleton.CurrentSyncedBlock - offset;
        else
            height = 1 + offset;
        
        for (int i = height ; count > 0; count--)
        {
            SimplifiedBlock? block = _smpBlocksCache.GetSimplifiedBlock(height);

            if(block is null)
            {
                rawBlocksList.Add(_nodeRequester.GetBlock((uint)height, ct, simplifiedTxInfo: 1)!); 
                await Task.Delay(160, ct);
            }
            else
                blocksList.Add(block);

            _ = sort == SortDirection.DESC ? i-- : i++;

            if(ct.IsCancellationRequested)
                return null!;
        }

        try
        {
            await Task.WhenAll(rawBlocksList);
        }
        catch
        {
            _logger.LogError("failed to get SimplifiedBlock list");
        }

        foreach (var rawBlock in rawBlocksList)
        {
            blocksList.Add(_blocksService.RPCBlockToSimplified(rawBlock.Result.Result!));
        }

        if(sort == SortDirection.DESC)
            blocksList.OrderByDescending(x => x.Height);
        else
            blocksList.OrderBy(x => x.Height);

        if(_smpBlocksCache.IsInCacheRange(blocksList[0].Height))
        {
            foreach (var block in rawBlocksList)
            {
                if(block is null || block.Result.Result is null)
                    continue;
               _smpBlocksCache.SetBlockCache(block.Result.Result);
            }
        }

        return blocksList;
    }

    public async Task<int?> ProbeBlockByHashAsync(string hash, CancellationToken cancellationToken = default)
    {
        GetBlock? rawBlock = await _cache.GetCachedBlockAsync<GetBlock>(hash, cancellationToken);

        if(rawBlock is null || rawBlock.Result is not null || rawBlock.Result!.Height <= 0)
            rawBlock = await _nodeRequester.GetBlock(hash, cancellationToken, 1);

        return rawBlock!.Result!.Height;
    }

    public async Task<string?> ProbeHashByHeightAsync(int height, CancellationToken cancellationToken = default)
    {
        GetBlockHash? hash = new()
        {
            Result = await _cache.GetCachedBlockHashAsync(height, cancellationToken)
        };

        hash ??= await _nodeRequester.GetBlockHash((uint)height, cancellationToken);
        
        return hash!.Result;
    }
}