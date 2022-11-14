using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.SignalR;
using ExplorerBackend.Hubs;
using ExplorerBackend.Configs;
using ExplorerBackend.Services.Caching;
using ExplorerBackend.Persistence.Repositories;
using ExplorerBackend.Models.API;
using ExplorerBackend.Models.Node;
using ExplorerBackend.Models.Node.Response;

namespace ExplorerBackend.Services.Workers;

public class BlocksWorker : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IHubContext<EventsHub> _hubContext;
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptionsMonitor<ExplorerConfig> _explorerConfig;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ChaininfoSingleton _chainInfoSingleton;
    private readonly IBlocksService _blocksService;

    public BlocksWorker(ILogger<BlocksWorker> logger, IHubContext<EventsHub> hubContext,
        IServiceProvider serviceProvider, IOptionsMonitor<ExplorerConfig> explorerConfig,
        IHttpClientFactory httpClientFactory, ChaininfoSingleton chaininfoSingleton,
        IBlocksService blocksService)
    {
        _logger = logger;
        _hubContext = hubContext;
        _serviceProvider = serviceProvider;
        _explorerConfig = explorerConfig;
        _httpClientFactory = httpClientFactory;
        _chainInfoSingleton = chaininfoSingleton;
        _blocksService = blocksService;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var httpClient = _httpClientFactory.CreateClient();

        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node);
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node.Url);
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node.Username);
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node.Password);

        httpClient.BaseAddress = new Uri(_explorerConfig.CurrentValue.Node.Url);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_explorerConfig.CurrentValue.Node.Username}:{_explorerConfig.CurrentValue.Node.Password}")));

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // set initial state, this fixes issue when after restart blocks not retrieved correctly from API before new block come from node
        using (var scope = _serviceProvider.CreateAsyncScope())
        {
            var blocksRepository = scope.ServiceProvider.GetRequiredService<IBlocksRepository>();
            var latestSyncedBlock = await blocksRepository.GetLatestBlockAsync(true, cancellationToken);

            if (latestSyncedBlock != null)
                _chainInfoSingleton.CurrentSyncedBlock = latestSyncedBlock.height;
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateAsyncScope())
                {
                    var blocksRepository = scope.ServiceProvider.GetRequiredService<IBlocksRepository>();
                    var transactionsRepository = scope.ServiceProvider.GetRequiredService<ITransactionsRepository>();
                    var rawTxsRepository = scope.ServiceProvider.GetRequiredService<IRawTxsRepository>();

                    var latestSyncedBlock = await blocksRepository.GetLatestBlockAsync(true, cancellationToken);
                    var currentIndexedBlock = (latestSyncedBlock != null ? latestSyncedBlock.height : 0) + 1;

                    for (var i = currentIndexedBlock; i < currentIndexedBlock + _explorerConfig.CurrentValue.BlocksPerBatch; i++)
                    {
                        try
                        {
                            // get hash by height
                            var getBlockHashRequest = new JsonRPCRequest
                            {
                                Id = 1,
                                Method = "getblockhash",
                                Params = new List<object>(new object[] { i })
                            };
                            var getBlockHashResponse = await httpClient.PostAsJsonAsync("", getBlockHashRequest, options, cancellationToken);
                            var blockHash = await getBlockHashResponse.Content.ReadFromJsonAsync<GetBlockHash>(options, cancellationToken);

                            if (blockHash == null || blockHash.Result == null)
                            {
                                //_logger.LogInformation("Can't pull block hash");
                                break;
                            }

                            // get block by hash
                            //blockHash.Result
                            var getBlockRequest = new JsonRPCRequest
                            {
                                Id = 1,
                                Method = "getblock",
                                Params = new List<object>(new object[] { blockHash.Result })
                            };
                            var getBlockResponse = await httpClient.PostAsJsonAsync("", getBlockRequest, options, cancellationToken);
                            var block = await getBlockResponse.Content.ReadFromJsonAsync<GetBlock>(options, cancellationToken);

                            if (block == null || block.Result == null)
                            {
                                _logger.LogInformation("Can't pull block");
                                break;
                            }

                            // save data to db
                            // check if block already exists in DB
                            var targetBlock = await blocksRepository.GetBlockByHeightAsync(i, cancellationToken);
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

                            // get block's transactions
                            var txFailed = await _blocksService.InsertTransactionsAsync(i, block.Result.Tx, cancellationToken);
                            if (txFailed) break;

                            if (!await blocksRepository.SetBlockSyncStateAsync(i, true, cancellationToken))
                            {
                                _logger.LogError(null, "Can't update block #{blockNumber}", i);
                                break;
                            }

                            try
                            {
                                _chainInfoSingleton.CurrentSyncedBlock = targetBlock.height;
                                await _hubContext.Clients.Group(EventsHub.BlocksDataChannel).SendAsync("blocksUpdated", new SimplifiedBlock
                                {
                                    Height = targetBlock.height,
                                    Size = targetBlock.size,
                                    Weight = targetBlock.weight,
                                    ProofType = targetBlock.proof_type,
                                    Time = targetBlock.time,
                                    MedianTime = targetBlock.mediantime,
                                    TxCount = block.Result.Tx?.Count ?? 0
                                }, cancellationToken);

                                await _chainInfoSingleton.BlockchainDataSemaphore.WaitAsync(cancellationToken);
                                _chainInfoSingleton.BlockchainDataShouldBroadcast = true;
                                _chainInfoSingleton.BlockchainDataSemaphore.Release();

                            }
                            catch
                            {

                            }

                            // check orphans
                            // make sense to set BlocksOrphanCheck to zero on initial sync                                                  
                            for (var j = targetBlock.height; j > targetBlock.height - _explorerConfig.CurrentValue.BlocksOrphanCheck && j > 0; j--)
                            {
                                // get hash by height
                                var getBlockHashCheckRequest = new JsonRPCRequest
                                {
                                    Id = 1,
                                    Method = "getblockhash",
                                    Params = new List<object>(new object[] { j })
                                };
                                var getBlockHashCheckResponse = await httpClient.PostAsJsonAsync("", getBlockHashCheckRequest, options, cancellationToken);
                                var blockHashCheck = await getBlockHashCheckResponse.Content.ReadFromJsonAsync<GetBlockHash>(options, cancellationToken);

                                if (blockHashCheck == null || blockHashCheck.Result == null)
                                {
                                    //_logger.LogInformation("Can't pull block hash");
                                    continue;
                                }

                                // better to get blocks from db in single query, however this is not "hot path" so...
                                var blockFromDB = await blocksRepository.GetBlockByHeightAsync(j, cancellationToken);
                                if (blockFromDB?.hash_hex == blockHashCheck.Result)
                                    continue;

                                if (!await _blocksService.UpdateDbBlockAsync(j, blockHashCheck.Result, cancellationToken))
                                {
                                    _logger.LogError("Can't update orphan block #{blockNumber}", j);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Can't process block #{blockNumber}", i);
                            break;
                        }
                    }
                }
                // TimeSpan not reuired here since we use milliseconds, still put it there to change in future if required
                await Task.Delay(TimeSpan.FromMilliseconds(_explorerConfig.CurrentValue.PullBlocksDelay), cancellationToken);
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Can't handle blocks");
            }
        }
    }
}