using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ExplorerBackend.VeilStructs;
using ExplorerBackend.Models.API;
using ExplorerBackend.Configs;
using ExplorerBackend.Models.Data;
using ExplorerBackend.Services.Core;
using ExplorerBackend.Persistence.Repositories;

namespace ExplorerBackend.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Produces("application/json")]
public class BlockController : ControllerBase
{

    private readonly ILogger _logger;
    private readonly IOptions<APIConfig> _apiConfig;
    private readonly IBlocksRepository _blocksRepository;
    private readonly ITransactionsRepository _transactionsRepository;
    private readonly IRawTxsRepository _rawTxsRepository;
    private readonly IUtilityService _utilityService;

    public BlockController(ILogger<BlockController> logger, IOptions<APIConfig> apiConfig, IBlocksRepository blocksRepository, ITransactionsRepository transactionsRepository, IRawTxsRepository rawTxsRepository, IUtilityService utilityService)
    {
        _logger = logger;
        _apiConfig = apiConfig;
        _blocksRepository = blocksRepository;
        _transactionsRepository = transactionsRepository;
        _rawTxsRepository = rawTxsRepository;
        _utilityService = utilityService;
    }

    [HttpPost(Name = "GetBlock")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(List<BlockResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(BlockRequest body)
    {
        if (body.Offset < 0)
            return Problem("offset should be higher or equal to zero", statusCode: 400);
        if (body.Count < 1)
            return Problem("count should be more or equal to one", statusCode: 400);
        if (body.Count > _apiConfig.Value.MaxBlocksPullCount)
            return Problem($"count should be less or equal than {_apiConfig.Value.MaxBlocksPullCount}", statusCode: 400);

        var response = new BlockResponse
        {
            Found = false
        };

        Block? block = null;
        if (body.Hash != null && _utilityService.VerifyHex(body.Hash))
            block = await _blocksRepository.GetBlockByHashAsync(body.Hash);
        else if (body.Height != null)
            block = await _blocksRepository.GetBlockByHeightAsync(body.Height.Value);
        else
            return Problem($"count should be less or equal than {_apiConfig.Value.MaxBlocksPullCount}", statusCode: 400);

        if (block != null)
        {
            var nextBlockHash = await _blocksRepository.ProbeHashByHeight(block.height + 1);
            var prevBlockHash = await _blocksRepository.ProbeHashByHeight(block.height - 1);

            response.Found = true;
            response.Block = block;
            response.TxnCount = block.txnCount;

            var verHex = BitConverter.GetBytes(block.version);
            verHex = verHex.Reverse().ToArray();
            response.VersionHex = _utilityService.ToHex(verHex);

            if (nextBlockHash != null)
                response.NextBlock = new BlockBasicData
                {
                    Hash = nextBlockHash,
                    Height = block.height + 1
                };

            if (prevBlockHash != null)
                response.PrevBlock = new BlockBasicData
                {
                    Hash = prevBlockHash,
                    Height = block.height - 1
                };

            response.Transactions = new List<TransactionSimpleDecoded>();
            var rtxs = await _transactionsRepository.GetTransactionsForBlockAsync(block.height, body.Offset, body.Count);
            if (rtxs != null)
            {
                var dict = new Dictionary<string, VeilTransaction>();
                var requiredTxs = new List<string>();
                foreach (var rawTx in rtxs)
                {
                    if (rawTx.data == null) continue;
                    var tx = VeilSerialization.Deserialize<VeilTransaction>(rawTx.data, 0);
                    // collect required previous txs
                    tx.TxIn?.ForEach(ntxin =>
                    {
                        if (ntxin.PrevOut != null && !ntxin.PrevOut.IsNull())
                        {
                            var prevTxHex = _utilityService.ToHexReversed(ntxin.PrevOut?.Hash ?? new byte[] { });
                            if (!requiredTxs.Contains(prevTxHex)) requiredTxs.Add(prevTxHex);
                        }
                    });
                    dict.Add(rawTx.txid_hex ?? "", tx);
                }

                // fetch prevout txs
                if (requiredTxs.Count() > 0)
                {
                    var outTxs = await _rawTxsRepository.GetTransactionsByIdsAsync(requiredTxs);
                    if (outTxs != null)
                        foreach (var rawTx in outTxs)
                        {
                            var tx = VeilSerialization.Deserialize<VeilTransaction>(rawTx.Value, 0);

                            if (!dict.ContainsKey(rawTx.Key))
                                dict.Add(rawTx.Key, tx);
                        }
                }

                foreach (var rawTx in rtxs)
                {
                    if (rawTx.data == null) continue;
                    var tx = dict[rawTx.txid_hex ?? ""];

                    var txsimple = new TransactionSimpleDecoded();
                    txsimple.TxId = rawTx.txid_hex;
                    txsimple.IsZerocoinSpend = tx.IsZerocoinSpend();
                    txsimple.IsZerocoinMint = tx.IsZerocoinMint();
                    txsimple.IsCoinStake = tx.IsCoinStake();
                    txsimple.IsBasecoin = tx.IsBasecoin();
                    txsimple.Inputs = new List<TxVinSimpleDecoded>();
                    txsimple.Outputs = new List<TxVoutSimpleDecoded>();

                    if (tx.TxIn != null)
                        foreach (var txin in tx.TxIn)
                        {
                            var txinsimple = new TxVinSimpleDecoded();
                            txinsimple.Type = TxInType.DEFAULT;
                            if (txin.IsAnonInput())
                                txinsimple.Type = TxInType.ANON;
                            if (txin.IsZerocoinSpend())
                                txinsimple.Type = TxInType.ZEROCOIN_SPEND;

                            txinsimple.PrevOutTx = _utilityService.ToHexReversed(txin.PrevOut?.Hash ?? new byte[] { });
                            txinsimple.PrevOutNum = txin.PrevOut?.N ?? 0;
                            if (dict.ContainsKey(txinsimple.PrevOutTx))
                            {
                                var prevTx = dict[txinsimple.PrevOutTx];
                                if (prevTx != null && prevTx.TxOut != null && prevTx.TxOut.Count() > txinsimple.PrevOutNum)
                                {
                                    if (prevTx.TxOut[(int)txinsimple.PrevOutNum].OutputType == OutputTypes.OUTPUT_STANDARD)
                                    {
                                        txinsimple.PrevOutAmount = prevTx?.TxOut[(int)txinsimple.PrevOutNum].Amount ?? -1;
                                        txinsimple.PrevOutAddresses = prevTx?.TxOut[(int)txinsimple.PrevOutNum].GetAddresses();
                                    }
                                    else
                                    {
                                        txinsimple.PrevOutAddresses = prevTx?.TxOut[(int)txinsimple.PrevOutNum].GetAddresses();
                                        txinsimple.PrevOutAmount = -1;
                                    }
                                }
                            }
                            if (txin.IsZerocoinSpend())
                                txinsimple.PrevOutAmount = txin.ZeroCoinSpend;

                            if (tx.IsBasecoin())
                            {
                                long reward;
                                Budget.GetBlockRewards(block.height, out reward, out _, out _, out _);
                                txinsimple.PrevOutAmount = reward;
                            }


                            txsimple.Inputs.Add(txinsimple);
                        }


                    if (tx.TxOut != null)
                    {
                        var outIndex = 0;
                        foreach (var txout in tx.TxOut)
                        {
                            var txoutsimple = new TxVoutSimpleDecoded();

                            txnouttype scriptType = txnouttype.TX_NONSTANDARD;

                            if (txout.ScriptPubKey != null)
                            {
                                Converters.Solver(txout.ScriptPubKey, out scriptType, new List<byte[]>());
                                if (txout.ScriptPubKey.Hash != null && txout.ScriptPubKey.Hash.Length > 0)
                                    txoutsimple.IsOpreturn = txout.ScriptPubKey.Hash[0] == (byte)opcodetype.OP_RETURN;
                                else
                                    txoutsimple.IsOpreturn = false;
                            }
                            else
                                txoutsimple.IsOpreturn = false;

                            txoutsimple.Addresses = txout.GetAddresses();
                            txoutsimple.IsCoinBase = tx.IsBasecoin() && outIndex == 0;
                            txoutsimple.Amount = txout.Amount;
                            txoutsimple.Type = txout.OutputType;
                            txoutsimple.ScriptPubKeyType = scriptType;

                            txsimple.Outputs.Add(txoutsimple);

                            outIndex++;
                        }
                    }

                    response.Transactions.Add(txsimple);
                }
            }
        }

        return Ok(response);
    }
}
