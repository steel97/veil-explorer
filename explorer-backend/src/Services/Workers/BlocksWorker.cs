using System.Text.Json;
using System.Transactions;
using System.Net.Http.Headers;
using System.Globalization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.SignalR;
using explorer_backend.Hubs;
using explorer_backend.Configs;
using explorer_backend.Services.Caching;
using explorer_backend.Persistence.Repositories;
using explorer_backend.Models.API;
using explorer_backend.Models.Data;
using explorer_backend.Models.Node;
using explorer_backend.Models.Node.Response;

namespace explorer_backend.Services.Workers;

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
                using var scope = _serviceProvider.CreateAsyncScope();
                var blocksRepository = scope.ServiceProvider.GetRequiredService<IBlocksRepository>();
                var transactionsRepository = scope.ServiceProvider.GetRequiredService<ITransactionsRepository>();
                var txInputsRepository = scope.ServiceProvider.GetRequiredService<ITxInputsRepository>();
                var ringctInputsRepository = scope.ServiceProvider.GetRequiredService<IRingctInputsRepository>();
                var txOutputsRepository = scope.ServiceProvider.GetRequiredService<ITxOutputsRepository>();

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
                        var txIds = block.Result.Tx ?? new List<string>();
                        foreach (var txId in txIds)
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

                            var targetTx = await transactionsRepository.GetTransactionByIdAsync(tx.txid);
                            if (targetTx != null) continue;

                            try
                            {
                                using (var txscope = new TransactionScope(asyncFlowOption: TransactionScopeAsyncFlowOption.Enabled))
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
                                    var inputsCompleted = true;
                                    var inputsRCTCompleted = true;
                                    var outputsCompleted = true;


                                    if (tx.vin != null)
                                        for (var index = 0; index < tx.vin.Count(); index++)
                                        {
                                            var input = tx.vin[index];
                                            var txInput = new TxInput
                                            {

                                                txid_hex = tx.txid,
                                                input_index = index,
                                                type = input.type switch
                                                {
                                                    "anon" => TxInputType.ANON,
                                                    "zerocoinspend" => TxInputType.ZEROCOINSPEND,
                                                    _ => TxInputType.UNKNOWN
                                                },
                                                num_inputs = input.num_inputs,
                                                ring_size = input.ring_size,
                                                prev_txid_hex = input.txid,
                                                denomination = input.denomination,
                                                serial_hex = input.serial,
                                                pubcoin_hex = input.pubcoin,
                                                vout = input.vout,
                                                scriptsig_asm = input.scriptSig?.asm,
                                                scriptsig_hex = input.scriptSig?.hex,
                                                txinwitnes_hexes = input.txinwitness,
                                                sequence = input.sequence
                                            };
                                            var txInputUniqueId = await txInputsRepository.InsertTxInputAsync(txInput);
                                            if (txInputUniqueId == null)
                                            {
                                                inputsCompleted = false;
                                                break;
                                            }

                                            // ringct inputs
                                            if (input.ringct_inputs != null)
                                                foreach (var rctInput in input.ringct_inputs)
                                                {
                                                    var rctInputModel = new Models.Data.RingctInput();
                                                    rctInputModel.tx_input_id = txInputUniqueId.Value;
                                                    rctInputModel.txid_hex = rctInput.txid;
                                                    rctInputModel.vout_n = rctInput.vout_n;
                                                    var rctState = await ringctInputsRepository.InsertRingctInputAsync(rctInputModel);

                                                    if (rctState == null)
                                                    {
                                                        inputsRCTCompleted = false;
                                                        break;
                                                    }
                                                }
                                        }

                                    if (tx.vout != null)
                                        foreach (var txout in tx.vout)
                                        {
                                            var txOutputModel = new TxOutput();
                                            txOutputModel.txid_hex = tx.txid;
                                            txOutputModel.output_index = txout.output_index;
                                            txOutputModel.type = txout.type switch
                                            {
                                                "coinbase" => TxOutputType.COINBASE,
                                                "standard" => TxOutputType.STANDARD,
                                                "data" => TxOutputType.DATA,
                                                "blind" => TxOutputType.BLIND,
                                                "ringct" => TxOutputType.RINGCT,
                                                "unknown" => TxOutputType.UNKNOWN,
                                                _ => TxOutputType.UNKNOWN
                                            };
                                            txOutputModel.valuesat = txout.valueSat;
                                            txOutputModel.vout_n = txout.vout_n;
                                            txOutputModel.scriptpub_asm = txout.scriptPubKey?.asm;
                                            txOutputModel.scriptpub_hex = txout.scriptPubKey?.hex;
                                            txOutputModel.scriptpub_type = txout.scriptPubKey?.type switch
                                            {
                                                "nonstandard" => TxScriptPubType.TX_NONSTANDARD,
                                                "pubkey" => TxScriptPubType.TX_PUBKEY,
                                                "pubkeyhash" => TxScriptPubType.TX_PUBKEYHASH,
                                                "pubkeyhash256" => TxScriptPubType.TX_PUBKEYHASH256,
                                                "timelocked_pubkeyhash" => TxScriptPubType.TX_TIMELOCKED_PUBKEYHASH,
                                                "timelocked_pubkeyhash256" => TxScriptPubType.TX_TIMELOCKED_PUBKEYHASH256,
                                                "scripthash" => TxScriptPubType.TX_SCRIPTHASH,
                                                "scripthash256" => TxScriptPubType.TX_SCRIPTHASH256,
                                                "timelocked_scripthash" => TxScriptPubType.TX_TIMELOCKED_SCRIPTHASH,
                                                "timelocked_scripthash256" => TxScriptPubType.TX_TIMELOCKED_SCRIPTHASH256,
                                                "multisig" => TxScriptPubType.TX_MULTISIG,
                                                "timelocked_multisig" => TxScriptPubType.TX_TIMELOCKED_MULTISIG,
                                                "nulldata" => TxScriptPubType.TX_NULL_DATA,
                                                "witness_v0_keyhash" => TxScriptPubType.TX_WITNESS_V0_KEYHASH,
                                                "witness_v0_scripthash" => TxScriptPubType.TX_WITNESS_V0_SCRIPTHASH,
                                                "witness_unknown" => TxScriptPubType.TX_WITNESS_UNKNOWN,
                                                "zerocoinmint" => TxScriptPubType.TX_ZEROCOINMINT,
                                                _ => TxScriptPubType.UNKNOWN
                                            };
                                            txOutputModel.reqsigs = txout.scriptPubKey?.reqSigs ?? 0;
                                            txOutputModel.addresses = txout.scriptPubKey?.addresses;
                                            txOutputModel.data_hex = txout.data_hex;
                                            txOutputModel.ct_fee = txout.ct_fee.ToString(CultureInfo.InvariantCulture);
                                            txOutputModel.valuecommitment_hex = txout.valueCommitment;
                                            txOutputModel.pubkey_hex = txout.pubkey;

                                            var outState = await txOutputsRepository.InsertTxOutputAsync(txOutputModel);

                                            if (outState == null)
                                            {
                                                outputsCompleted = false;
                                                break;
                                            }
                                        }

                                    if (txCompleted && inputsCompleted && inputsRCTCompleted && outputsCompleted)
                                        txscope.Complete();
                                    else
                                        txFailed = true;
                                }
                            }

                            catch (TransactionAbortedException txex)
                            {
                                _logger.LogError(txex, $"Can't save transaction {tx} (insert) for block #{i}");
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

                // TimeSpan not reuired here since we use milliseconds, still put it there to change in future if required
                await Task.Delay(TimeSpan.FromMilliseconds(_explorerConfig.CurrentValue.PullBlocksDelay));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Can't handle blocks");
            }
        }
    }
}