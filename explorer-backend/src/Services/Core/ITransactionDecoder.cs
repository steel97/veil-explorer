using ExplorerBackend.Models.API;
using ExplorerBackend.Models.System;

namespace ExplorerBackend.Services.Core;

public interface ITransactionDecoder
{
    Task<List<TransactionSimpleDecoded>?> DecodeTransactionsAsync(List<TxDecodeTarget> targets, int blockHeight, CancellationToken cancellationToken);
}