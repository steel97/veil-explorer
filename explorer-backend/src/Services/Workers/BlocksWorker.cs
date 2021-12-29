using System.Text.Json;
using System.Transactions;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.SignalR;
using ExplorerBackend.Hubs;
using ExplorerBackend.Configs;
using ExplorerBackend.Services.Caching;
using ExplorerBackend.Persistence.Repositories;
using ExplorerBackend.Models.API;
using ExplorerBackend.Models.Data;
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

    public BlocksWorker(ILogger<BlocksWorker> logger, IHubContext<EventsHub> hubContext, IServiceProvider serviceProvider, IOptionsMonitor<ExplorerConfig> explorerConfig, IHttpClientFactory httpClientFactory, ChaininfoSingleton chaininfoSingleton)
    {
        _logger = logger;
        _hubContext = hubContext;
        _serviceProvider = serviceProvider;
        _explorerConfig = explorerConfig;
        _httpClientFactory = httpClientFactory;
        _chainInfoSingleton = chaininfoSingleton;
    }

    protected override async Task ExecuteAsync(CancellationToken stopToken)
    {
        using var httpClient = _httpClientFactory.CreateClient();

        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node);
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node.Url);
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node.Authorization);

        httpClient.BaseAddress = new Uri(_explorerConfig.CurrentValue.Node.Url);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _explorerConfig.CurrentValue.Node.Authorization);

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        while (!stopToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateAsyncScope())
                {
                    var blocksRepository = scope.ServiceProvider.GetRequiredService<IBlocksRepository>();
                    var transactionsRepository = scope.ServiceProvider.GetRequiredService<ITransactionsRepository>();
                    var rawTxsRepository = scope.ServiceProvider.GetRequiredService<IRawTxsRepository>();

                    var latestSyncedBlock = await blocksRepository.GetLatestBlockAsync(true);
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
                            var getBlockHashResponse = await httpClient.PostAsJsonAsync<JsonRPCRequest>("", getBlockHashRequest, options);
                            var blockHash = await getBlockHashResponse.Content.ReadFromJsonAsync<GetBlockHash>(options);

                            if (blockHash == null || blockHash.Result == null)
                            {
                                _logger.LogInformation("Can't pull block hash");
                                break;
                            }

                            // get block by hash
                            var getBlockRequest = new JsonRPCRequest
                            {
                                Id = 1,
                                Method = "getblock",
                                Params = new List<object>(new object[] { blockHash.Result })
                            };
                            var getBlockResponse = await httpClient.PostAsJsonAsync<JsonRPCRequest>("", getBlockRequest, options);
                            var block = await getBlockResponse.Content.ReadFromJsonAsync<GetBlock>(options);

                            if (block == null || block.Result == null)
                            {
                                _logger.LogInformation("Can't pull block");
                                break;
                            }

                            // get block's transactions
                            var pulledTxs = new List<GetRawTransactionResult>();
                            if (block.Result.Tx != null)
                                foreach (var txId in block.Result.Tx)
                                {
                                    var getTxRequest = new JsonRPCRequest
                                    {
                                        Id = 1,
                                        Method = "getrawtransaction",
                                        Params = new List<object>(new object[] { txId, true })
                                    };
                                    var getTxResponse = await httpClient.PostAsJsonAsync<JsonRPCRequest>("", getTxRequest, options);
                                    var tx = await getTxResponse.Content.ReadFromJsonAsync<GetRawTransaction>(options);

                                    if (tx == null || tx.Result == null)
                                    {
                                        _logger.LogInformation($"Can't pull transaction {txId} for block #{i}");
                                        break;
                                    }

                                    pulledTxs.Add(tx.Result);
                                }

                            // save data to db
                            // check if block already exists in DB
                            var targetBlock = await blocksRepository.GetBlockByHeightAsync(i);
                            // transform block rpc to block data
                            if (targetBlock == null)
                            {
                                targetBlock = new Block();
                                targetBlock.anon_index = block.Result.Anon_index;
                                targetBlock.bits_hex = block.Result.Bits;
                                targetBlock.chainwork_hex = block.Result.Chainwork;
                                targetBlock.difficulty = block.Result.Difficulty;
                                targetBlock.epoch_number = block.Result.epoch_number;
                                targetBlock.hash_hex = block.Result.Hash;
                                targetBlock.height = block.Result.Height;
                                targetBlock.mediantime = block.Result.Mediantime;
                                targetBlock.merkleroot_hex = block.Result.Merkleroot;
                                targetBlock.mixhash_hex = block.Result.Mixhash;
                                targetBlock.nonce = block.Result.Nonce;
                                targetBlock.nonce64 = block.Result.Nonce64;
                                targetBlock.prog_header_hash_hex = block.Result.prog_header_hash;
                                targetBlock.prog_header_hex = block.Result.prog_header;
                                targetBlock.progpowmixhash_hex = block.Result.progpowmixhash;
                                targetBlock.progproofofworkhash_hex = block.Result.progproofofworkhash;
                                targetBlock.proofofstakehash_hex = block.Result.Proofofstakehash;
                                targetBlock.proofofworkhash_hex = block.Result.proofofworkhash;
                                targetBlock.randomxproofofworkhash_hex = block.Result.randomxproofofworkhash;
                                targetBlock.sha256dproofofworkhash_hex = block.Result.sha256dproofofworkhash;
                                targetBlock.size = block.Result.Size;
                                targetBlock.strippedsize = block.Result.Strippedsize;
                                targetBlock.time = block.Result.Time;
                                targetBlock.proof_type = block.Result.Proof_type switch
                                {
                                    "Proof-of-Work (X16RT)" => BlockType.POW_X16RT,
                                    "Proof-of-work (ProgPow)" => BlockType.POW_ProgPow,
                                    "Proof-of-work (RandomX)" => BlockType.POW_RandomX,
                                    "Proof-of-work (Sha256D)" => BlockType.POW_Sha256D,
                                    "Proof-of-Stake" => BlockType.POS,
                                    _ => BlockType.UNKNOWN
                                };
                                targetBlock.veil_data_hash_hex = block.Result.Veil_data_hash;
                                targetBlock.version = block.Result.Version;
                                targetBlock.weight = block.Result.Weight;
                                targetBlock.synced = false;

                                // save block
                                if (!await blocksRepository.InsertBlockAsync(targetBlock))
                                {
                                    _logger.LogError(null, $"Can't save block #{i}");
                                    break;
                                }
                            }

                            var txFailed = false;
                            foreach (var tx in pulledTxs)
                            {
                                ArgumentNullException.ThrowIfNull(tx);
                                ArgumentNullException.ThrowIfNull(tx.txid);
                                ArgumentNullException.ThrowIfNull(tx.hex);

                                var targetTx = await transactionsRepository.GetTransactionByIdAsync(tx.txid);
                                if (targetTx != null) continue;

                                try
                                {
                                    using (var txscope = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromSeconds(_explorerConfig.CurrentValue.TxScopeTimeout), TransactionScopeAsyncFlowOption.Enabled))
                                    {
                                        targetTx = new Models.Data.Transaction();
                                        targetTx.txid_hex = tx.txid;
                                        targetTx.hash_hex = tx.hash;
                                        targetTx.version = tx.version;
                                        targetTx.size = tx.size;
                                        targetTx.vsize = tx.vsize;
                                        targetTx.weight = tx.weight;
                                        targetTx.locktime = tx.locktime;
                                        targetTx.block_height = i;

                                        var txCompleted = await transactionsRepository.InsertTransactionAsync(targetTx);
                                        var txRawCompleted = await rawTxsRepository.InsertTransactionAsync(tx.txid, tx.hex);

                                        if (txCompleted && txRawCompleted)
                                            txscope.Complete();
                                        else
                                            txFailed = true;
                                    }
                                }

                                catch (TransactionAbortedException txex)
                                {
                                    _logger.LogError(txex, $"Can't save transaction {tx.txid} (insert) for block #{i}");
                                    txFailed = true;
                                    break;
                                }
                            }

                            if (txFailed) break;

                            if (!await blocksRepository.SetBlockSyncStateAsync(i, true))
                            {
                                _logger.LogError(null, $"Can't update block #{i}");
                                break;
                            }

                            try
                            {
                                _chainInfoSingleton.CurrentSyncedBlock = targetBlock.height;
                                await _hubContext.Clients.Group("blocksupdate").SendAsync("BlocksUpdated", new SimplifiedBlock
                                {
                                    Height = targetBlock.height,
                                    Size = targetBlock.size,
                                    Weight = targetBlock.weight,
                                    ProofType = targetBlock.proof_type,
                                    Time = targetBlock.time,
                                    MedianTime = targetBlock.mediantime,
                                    TxCount = pulledTxs.Count()
                                });
                            }
                            catch
                            {

                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Can't process block #{i}");
                            break;
                        }
                    }
                }
                // TimeSpan not reuired here since we use milliseconds, still put it there to change in future if required
                await Task.Delay(TimeSpan.FromMilliseconds(_explorerConfig.CurrentValue.PullBlocksDelay));
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