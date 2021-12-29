using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ExplorerBackend.VeilStructs;
using ExplorerBackend.Models.API;
using ExplorerBackend.Configs;
using ExplorerBackend.Services.Core;
using ExplorerBackend.Services.Caching;
using ExplorerBackend.Persistence.Repositories;

namespace ExplorerBackend.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Produces("application/json")]
public class UnconfirmedTransactions : ControllerBase
{

    private readonly ILogger _logger;
    private readonly IOptions<APIConfig> _apiConfig;
    private readonly ChaininfoSingleton _chaininfoSingleton;
    private readonly IRawTxsRepository _rawTxsRepository;
    private readonly IUtilityService _utilityService;

    public UnconfirmedTransactions(ILogger<UnconfirmedTransactions> logger, IOptions<APIConfig> apiConfig, ChaininfoSingleton chaininfoSingleton, IRawTxsRepository rawTxsRepository, IUtilityService utilityService)
    {
        _logger = logger;
        _apiConfig = apiConfig;
        _chaininfoSingleton = chaininfoSingleton;
        _rawTxsRepository = rawTxsRepository;
        _utilityService = utilityService;
    }

    [HttpGet(Name = "UnconfirmedTransactions")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(UnconfirmedTxResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(int offset, int count)
    {
        if (offset < 0)
            return Problem("offset should be higher or equal to zero", statusCode: 400);
        if (count < 1)
            return Problem("count should be more or equal to one", statusCode: 400);
        if (count > _apiConfig.Value.MaxTransactionsPullCount)
            return Problem($"count should be less or equal than {_apiConfig.Value.MaxBlocksPullCount}", statusCode: 400);

        var txncount = _chaininfoSingleton.UnconfirmedTxs?.Count() ?? 0;

        List<TransactionSimpleDecoded>? txs = null;

        if (_chaininfoSingleton.UnconfirmedTxs != null)
        {
            if (_chaininfoSingleton.UnconfirmedTxs.Count() > offset)
            {
                txs = new List<TransactionSimpleDecoded>();
                var reqTxs = _chaininfoSingleton.UnconfirmedTxs.Skip(offset).Take(count);
                var requiredTxs = new List<string>();
                var dict = new Dictionary<string, VeilTransaction>();
                foreach (var txdata in reqTxs)
                {
                    if (txdata == null || txdata.hex == null) continue;
                    var tx = VeilSerialization.Deserialize<VeilTransaction>(_utilityService.HexToByteArray(txdata.hex), 0);
                    // collect required previous txs
                    tx.TxIn?.ForEach(ntxin =>
                    {
                        if (ntxin.PrevOut != null && !ntxin.PrevOut.IsNull())
                        {
                            var prevTxHex = _utilityService.ToHexReversed(ntxin.PrevOut?.Hash ?? new byte[] { });
                            if (!requiredTxs.Contains(prevTxHex)) requiredTxs.Add(prevTxHex);
                        }
                    });
                    dict.Add(txdata.txid ?? "", tx);
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


                foreach (var rawTx in reqTxs)
                {
                    if (rawTx == null || rawTx.hex == null) continue;
                    var data = _utilityService.HexToByteArray(rawTx.hex);
                    if (data == null) continue;
                    var tx = dict[rawTx.txid ?? ""];

                    var txsimple = new TransactionSimpleDecoded();
                    txsimple.TxId = rawTx.txid;
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
                                Budget.GetBlockRewards((int)((_chaininfoSingleton.currentChainInfo?.Blocks ?? 0) + 1), out reward, out _, out _, out _);
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

                    txs.Add(txsimple);
                }

            }
        }

        return Ok(new UnconfirmedTxResponse
        {
            TxnCount = txncount,
            Transactions = txs
        });
    }
}
