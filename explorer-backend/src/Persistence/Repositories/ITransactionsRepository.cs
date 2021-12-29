using ExplorerBackend.Models.Data;

namespace ExplorerBackend.Persistence.Repositories;

public interface ITransactionsRepository
{
    Task<Transaction?> GetTransactionByIdAsync(string txid);
    Task<List<TransactionExtended>?> GetTransactionsForBlockAsync(int blockHeight, int offset, int count);
    Task<string?> ProbeTransactionByHashAsync(string txid);
    Task<bool> InsertTransactionAsync(Transaction txTemplate);
}