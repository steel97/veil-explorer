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
        int height = sort == SortDirection.DESC ? _chaininfoSingleton.CurrentSyncedBlock - offset : 1 + offset + (count - 1);

        bool IsInCacheRange = _smpBlocksCache.IsInCacheRange(height);

        List<SimplifiedBlock> blocksList = new(count);

        if(IsInCacheRange)
        {
            _smpBlocksCache.GetSimplifiedBlocksRange(height, count, blocksList, out List<byte> notCachedBlockOffset);

            if(notCachedBlockOffset.Count > 0)            
                await GetBlocksViaRPC(height, count, blocksList, ct, notCachedBlockOffset);            
        }
        else
        {
            await GetBlocksViaRPC(height, count, blocksList, ct);
        }

        if(sort == SortDirection.DESC)
            blocksList.OrderByDescending(x => x.Height);
        else
            blocksList.OrderBy(x => x.Height);

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

    private async Task GetBlocksViaRPC(int height, int count, List<SimplifiedBlock> blocksList, CancellationToken ct, List<byte>? list = null)
    {
        List<Task<GetBlock>> rawBlocksList = new(count);

        try
        {
            if(list is not null && list.Count > 0)
            {
                for(int i = 0; i < list.Count; i++)
                {
                    rawBlocksList.Add(_nodeRequester.GetBlock((uint)height - list[i], ct)!);
                }
                if(ct.IsCancellationRequested)
                    return;
            }
            else
            {
                for (int i = height ; count > 0; count--, i--)
                {
                    rawBlocksList.Add(_nodeRequester.GetBlock((uint)i, ct, simplifiedTxInfo: 1)!);

                    if(ct.IsCancellationRequested)
                        return;
                }
            }
            await Task.WhenAll(rawBlocksList);
        }
        catch
        {
            _logger.LogError("{service} failed to get list of simplified blocks :(", nameof(RealtimeBlocksDataService));
        }

        foreach (var item in rawBlocksList)
        {
            if(item.Result is not null && item.Result.Result is not null)
                blocksList.Add(_blocksService.RPCBlockToSimplified(item.Result.Result));
        }
    }
}