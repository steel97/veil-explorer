using explorer_backend.Models.Data;

namespace explorer_backend.Persistence.Repositories;

public interface ITransactionsRepository
{
    Task<Transaction?> GetTransactionByIdAsync(string txid);
    Task<bool> InsertTransactionAsync(Transaction txTemplate);
}