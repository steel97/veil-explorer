using ExplorerBackend.Models.Data;

namespace ExplorerBackend.Persistence.Repositories;

public interface ITransactionsRepository
{
    Task<Transaction?> GetTransactionByIdAsync(string txid, CancellationToken cancellationToken = default(CancellationToken));
    Task<TransactionExtended?> GetTransactionFullByIdAsync(string txid, CancellationToken cancellationToken = default(CancellationToken));
    Task<List<TransactionExtended>?> GetTransactionsForBlockAsync(int blockHeight, int offset, int count, CancellationToken cancellationToken = default(CancellationToken));
    Task<string?> ProbeTransactionByHashAsync(string txid, CancellationToken cancellationToken = default(CancellationToken));
    Task<bool> InsertTransactionAsync(Transaction txTemplate, CancellationToken cancellationToken = default(CancellationToken));
}