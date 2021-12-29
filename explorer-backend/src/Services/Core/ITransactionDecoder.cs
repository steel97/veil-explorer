using ExplorerBackend.Models.API;
using ExplorerBackend.Models.System;

namespace ExplorerBackend.Services.Core;

public interface ITransactionDecoder
{
    Task<List<TransactionSimpleDecoded>?> DecodeTransactions(List<TxDecodeTarget> targets, int blockHeight);
}