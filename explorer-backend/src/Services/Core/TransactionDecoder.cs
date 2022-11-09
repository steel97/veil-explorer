using ExplorerBackend.VeilStructs;
using ExplorerBackend.Models.API;
using ExplorerBackend.Persistence.Repositories;
using ExplorerBackend.Models.System;

namespace ExplorerBackend.Services.Core;

public class TransactionsDecoder : ITransactionDecoder
{
    private readonly IRawTxsRepository _rawTxsRepository;
    private readonly IUtilityService _utilityService;

    public TransactionsDecoder(IRawTxsRepository rawTxsRepository, IUtilityService utilityService)
    {

        _rawTxsRepository = rawTxsRepository;
        _utilityService = utilityService;
    }

    public async Task<List<TransactionSimpleDecoded>?> DecodeTransactionsAsync(List<TxDecodeTarget> targets, int blockHeight, CancellationToken cancellationToken)
    {
        var txs = new List<TransactionSimpleDecoded>();


        var dict = new Dictionary<string, VeilTransaction>();
        var requiredTxs = new List<string>();
        foreach (var rawTx in targets)
        {
            if (rawTx.Data == null) continue;
            var tx = VeilSerialization.Deserialize<VeilTransaction>(rawTx.Data, 0);
            // collect required previous txs
            tx.TxIn?.ForEach(ntxin =>
            {
                if (ntxin.PrevOut != null && !ntxin.PrevOut.IsNull())
                {
                    var prevTxHex = _utilityService.ToHexReversed(ntxin.PrevOut?.Hash ?? Array.Empty<byte>());
                    if (!requiredTxs.Contains(prevTxHex)) requiredTxs.Add(prevTxHex);
                }
            });
            dict.Add(rawTx.TxId ?? "", tx);
        }

        // fetch prevout txs
        if (requiredTxs.Count > 0)
        {
            var outTxs = await _rawTxsRepository.GetTransactionsByIdsAsync(requiredTxs, cancellationToken);
            if (outTxs != null)
                foreach (var rawTx in outTxs)
                {
                    var tx = VeilSerialization.Deserialize<VeilTransaction>(rawTx.Value, 0);

                    if (!dict.ContainsKey(rawTx.Key))
                        dict.Add(rawTx.Key, tx);
                }
        }

        foreach (var rawTx in targets)
        {
            if (rawTx.Data == null) continue;
            var tx = dict[rawTx.TxId ?? ""];

            var txsimple = new TransactionSimpleDecoded
            {
                TxId = rawTx.TxId,
                IsZerocoinSpend = tx.IsZerocoinSpend(),
                IsZerocoinMint = tx.IsZerocoinMint(),
                IsCoinStake = tx.IsCoinStake(),
                IsBasecoin = tx.IsBasecoin(),
                Inputs = new List<TxVinSimpleDecoded>(),
                Outputs = new List<TxVoutSimpleDecoded>()
            };

            if (tx.TxIn != null)
                foreach (var txin in tx.TxIn)
                {
                    var txinsimple = new TxVinSimpleDecoded
                    {
                        Type = TxInType.DEFAULT
                    };
                    if (txin.IsAnonInput())
                        txinsimple.Type = TxInType.ANON;
                    if (txin.IsZerocoinSpend())
                        txinsimple.Type = TxInType.ZEROCOIN_SPEND;

                    txinsimple.PrevOutTx = _utilityService.ToHexReversed(txin.PrevOut?.Hash ?? Array.Empty<byte>());
                    txinsimple.PrevOutNum = txin.PrevOut?.N ?? 0;
                    if (dict.TryGetValue(txinsimple.PrevOutTx, out VeilTransaction? value))
                    {
                        var prevTx = value;
                        if (prevTx != null && prevTx.TxOut != null && prevTx.TxOut.Count > txinsimple.PrevOutNum)
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
                        VeilBudget.GetBlockRewards(blockHeight, out long reward, out _, out _, out _);
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

                    var scriptType = txnouttype.TX_NONSTANDARD;

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
                    txoutsimple.CTFee = null;

                    if (txout.OutputType == OutputTypes.OUTPUT_DATA)
                    {
                        txoutsimple.CTFee = ((VeilTxOutData)txout).CTFee;
                    }

                    txsimple.Outputs.Add(txoutsimple);

                    outIndex++;
                }
            }

            txs.Add(txsimple);
        }


        return txs;
    }
}