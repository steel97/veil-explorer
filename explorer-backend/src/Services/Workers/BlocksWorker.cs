using System.Text;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.SignalR;
using ExplorerBackend.Hubs;
using ExplorerBackend.Configs;
using ExplorerBackend.Services.Caching;
using ExplorerBackend.Persistence.Repositories;
using ExplorerBackend.Models.API;
using ExplorerBackend.Services.Core;
using ExplorerBackend.Models.Node.Response;
using ExplorerBackend.Models.Data;

namespace ExplorerBackend.Services.Workers;

public class BlocksWorker : BackgroundService
{
    private readonly int _blocksPerBatch;
    private readonly int _blocksOrphanCheck;
    private readonly bool _RPCMode;
    private readonly ILogger _logger;
    private readonly IHubContext<EventsHub> _hubContext;
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptionsMonitor<ExplorerConfig> _explorerConfig;
    private readonly ChaininfoSingleton _chainInfoSingleton;
    private readonly BlocksCacheSingleton _blocksCacheSingleton;
    private readonly SimplifiedBlocksCacheSingleton _simplifiedBlocksCacheSingleton;
    private readonly NodeRequester _nodeRequester;
    private readonly IBlocksService _blocksService;

    public BlocksWorker(ILogger<BlocksWorker> logger, IHubContext<EventsHub> hubContext, IServiceProvider serviceProvider,
        IOptionsMonitor<ExplorerConfig> explorerConfig, ChaininfoSingleton chaininfoSingleton,
        BlocksCacheSingleton blocksCacheSingleton, SimplifiedBlocksCacheSingleton simplifiedBlocksCacheSingleton, 
        NodeRequester nodeRequester, IBlocksService blocksService)
    {
        _logger = logger;
        _hubContext = hubContext;
        _serviceProvider = serviceProvider;
        _explorerConfig = explorerConfig;
        _chainInfoSingleton = chaininfoSingleton;
        _blocksCacheSingleton = blocksCacheSingleton;
        _simplifiedBlocksCacheSingleton = simplifiedBlocksCacheSingleton;
        _nodeRequester = nodeRequester;
        _blocksService = blocksService;
        _blocksPerBatch = _explorerConfig.CurrentValue.BlocksPerBatch == 0 ? 10 : _explorerConfig.CurrentValue.BlocksPerBatch;
        _blocksOrphanCheck = _explorerConfig.CurrentValue.BlocksOrphanCheck == 0 ? 12 : _explorerConfig.CurrentValue.BlocksOrphanCheck;
        _RPCMode = _explorerConfig.CurrentValue.RPCMode;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (_RPCMode)
        {
            try
            {
                var latestBlock = await _nodeRequester.GetLatestBlock(cancellationToken);
               
                if (latestBlock != null || latestBlock!.Result != null)
                {                
                    _chainInfoSingleton.CurrentSyncedBlock = latestBlock.Result!.Height;
                    _simplifiedBlocksCacheSingleton.SetBlockCache(latestBlock.Result, true);
                }
            }
            catch (Exception)
            {
                _logger.LogError("Can't get blocks");
            }
        }
        else
        {
            // set initial state, this fixes issue when after restart blocks not retrieved correctly from API before new block come from node
            await using var scope = _serviceProvider.CreateAsyncScope();
            var blocksRepository = scope.ServiceProvider.GetRequiredService<IBlocksRepository>();
            var latestSyncedBlock = await blocksRepository.GetLatestBlockAsync(true, cancellationToken);

            if (latestSyncedBlock != null)
                _chainInfoSingleton.CurrentSyncedBlock = latestSyncedBlock.height;
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (_explorerConfig.CurrentValue.RPCMode)
                {
                    await RPCModeIteration(cancellationToken);
                }
                else
                {
                    await DBModeIteration(cancellationToken);
                }

                // TimeSpan not reuired here since we use milliseconds, still put it there to change in future if required
                await Task.Delay(TimeSpan.FromMilliseconds(_explorerConfig.CurrentValue.PullBlocksDelay), cancellationToken);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Can't handle blocks");
            }
        }
    }

    private async Task DBModeIteration(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateAsyncScope();

        var blocksRepository = scope.ServiceProvider.GetRequiredService<IBlocksRepository>();
        var transactionsRepository = scope.ServiceProvider.GetRequiredService<ITransactionsRepository>();
        var rawTxsRepository = scope.ServiceProvider.GetRequiredService<IRawTxsRepository>();

        var latestSyncedBlock = await blocksRepository.GetLatestBlockAsync(true, cancellationToken);
        int currentIndexedBlock = (latestSyncedBlock != null ? latestSyncedBlock.height : 0) + 1;

        for (int i = currentIndexedBlock; i < currentIndexedBlock + _blocksPerBatch; i++)
        {
            try
            {
                var blockHash = await _nodeRequester.GetBlockHash((uint)i, cancellationToken);

                if (blockHash == null || blockHash.Result == null)
                    break;

                var block = await _nodeRequester.GetBlock(blockHash.Result, cancellationToken, simplifiedTxInfo: 2); // 1 -> block, 2 -> block + tx

                if (block == null || block.Result == null)
                {
                    _logger.LogInformation("Can't pull block");
                    break;
                }

                // save data to db
                // check if block already exists in DB
                var targetBlock = await blocksRepository.GetBlockAsync(i, cancellationToken);
                // transform block rpc to block data
                if (targetBlock == null)
                {
                    targetBlock = _blocksService.RPCBlockToDb(block.Result);
                    // save block
                    if (!await blocksRepository.InsertBlockAsync(targetBlock, cancellationToken))
                    {
                        _logger.LogError(null, "Can't save block #{blockNumber}", i);
                        break;
                    }
                }

                // save block's transactions
                var txFailed = await _blocksService.InsertTransactionsAsync(i, block.Result.Txs!, cancellationToken);
                if (txFailed) break;
  
                if (!await blocksRepository.SetBlockSyncStateAsync(i, true, cancellationToken))
                {
                    _logger.LogError(null, "Can't update block #{blockNumber}", i);
                    break;
                }

                try
                {
                    await OnNewBlockHubUpdate(block, block.Result.NTx, cancellationToken);
                }
                catch { }
                // check orphans
                // make sense to set BlocksOrphanCheck to zero on initial sync
                for (int j = targetBlock.height - _blocksOrphanCheck; j < targetBlock.height; j++)
                {
                    var blockHashCheck = await _nodeRequester.GetBlockHash((uint)j, cancellationToken);

                    if (blockHashCheck == null || blockHashCheck.Result == null)
                        continue;

                    // better to get blocks from db in single query, however this is not "hot path" so...
                    var blockFromDB = await blocksRepository.GetBlockAsync(j, cancellationToken);
                    if (blockFromDB?.hash_hex == blockHashCheck.Result)
                        continue;

                    if (!await _blocksService.UpdateDbBlockAsync(j, blockHashCheck.Result, cancellationToken))
                        _logger.LogError("Can't update orphan block #{blockNumber}", j);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Can't process block #{blockNumber}", i);
                break;
            }
        }
    }

    private async Task RPCModeIteration(CancellationToken cancellationToken)
    {
        try
        {   
            var block = await _nodeRequester.GetBlockHash((uint)_chainInfoSingleton.CurrentSyncedBlock, cancellationToken);
            if (block is null || block.Result is null)
                return;
            
            var newBlock = await _nodeRequester.GetBlock(block.Result, cancellationToken);
            if(newBlock is null || newBlock.Result is null)
                return;
            
            _simplifiedBlocksCacheSingleton.SetBlockCache(newBlock.Result, true);
            var setBlockCache = _blocksCacheSingleton.SetServerCacheDataAsync(newBlock.Result.Height, newBlock.Result.Hash!, newBlock.Result, cancellationToken);
            var updateHub = OnNewBlockHubUpdate(newBlock, newBlock.Result.NTx, cancellationToken);

            try
            {
                await Task.WhenAll(setBlockCache, updateHub);
            }
            catch { }
            // checking prev blocks
            for (int i = _chainInfoSingleton.CurrentSyncedBlock - _blocksOrphanCheck; i < _chainInfoSingleton.CurrentSyncedBlock; i++)
            {
                var prevBlocksHash = await _nodeRequester.GetBlockHash((uint)i, cancellationToken);

                if (prevBlocksHash is null || prevBlocksHash.Result is null)
                    continue;
                
                bool isPrevBlockHashValid = await _blocksCacheSingleton.ValidateCacheAsync(i.ToString(), prevBlocksHash.Result);

                if(isPrevBlockHashValid)
                {
                    var prevBlock = await _nodeRequester.GetBlock(prevBlocksHash.Result, cancellationToken, 2);

                    if(prevBlock is null || prevBlock.Result is null)
                        continue;

                    _simplifiedBlocksCacheSingleton.SetBlockCache(prevBlock.Result);
                    var updateCache = _blocksCacheSingleton.UpdateCachedDataAsync(i.ToString(), prevBlocksHash.Result, prevBlock.Result, cancellationToken);

                    await Task.WhenAll(updateCache);
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Can't pull new block");
        }
    }

    private async Task OnNewBlockHubUpdate(GetBlock block, int txCount, CancellationToken cancellationToken)
    {
        _chainInfoSingleton.CurrentSyncedBlock = block.Result!.Height;
        await _hubContext.Clients.Group(EventsHub.BlocksDataChannel).SendAsync("blocksUpdated", new SimplifiedBlock
        {
            Height = block.Result.Height,
            Size = block.Result.Size,
            Weight = block.Result.Weight,
            ProofType = _blocksService.GetBlockType(block.Result.Proof_type!),
            Time = block.Result.Time,
            MedianTime = block.Result.Mediantime,
            TxCount = txCount
        }, cancellationToken);

        await _chainInfoSingleton.BlockchainDataSemaphore.WaitAsync(cancellationToken);
        _chainInfoSingleton.BlockchainDataShouldBroadcast = true;
        _chainInfoSingleton.BlockchainDataSemaphore.Release();
    }
}