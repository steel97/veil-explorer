using ExplorerBackend.Models.Data;

namespace ExplorerBackend.Persistence.Repositories;

public interface ITransactionsRepository
{
    Task<Transaction?> GetTransactionByIdAsync(string txid);
    Task<string?> ProbeTransactionByHashAsync(string txid);
    Task<bool> InsertTransactionAsync(Transaction txTemplate);
}