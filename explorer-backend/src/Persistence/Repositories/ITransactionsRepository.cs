using ExplorerBackend.Models.Data;

namespace ExplorerBackend.Persistence.Repositories;

public interface ITransactionsRepository
{
    Task<Transaction?> GetTransactionByIdAsync(string txid, CancellationToken cancellationToken = default);
    Task<TransactionExtended?> GetTransactionFullByIdAsync(string txid, CancellationToken cancellationToken = default);
    Task<List<TransactionExtended>?> GetTransactionsForBlockAsync(int blockHeight, int offset, int count, bool fetchAll, CancellationToken cancellationToken = default);
    Task<bool> RemoveTransactionsForBlockAsync(int blockHeight, CancellationToken cancellationToken = default);
    Task<string?> ProbeTransactionByHashAsync(string txid, CancellationToken cancellationToken = default);
    Task<bool> InsertTransactionAsync(Transaction txTemplate, CancellationToken cancellationToken = default);
}