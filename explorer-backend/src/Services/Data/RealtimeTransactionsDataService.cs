using ExplorerBackend.Models.Data;

namespace ExplorerBackend.Services.Data;

public class RealtimeTransactionsDataService : ITransactionsDataService
{
    public Task<Transaction?> GetTransactionByIdAsync(string txid, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<TransactionExtended?> GetTransactionFullByIdAsync(string txid, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<List<TransactionExtended>?> GetTransactionsForBlockAsync(int blockHeight, int offset, int count, bool fetchAll, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<string?> ProbeTransactionByHashAsync(string txid, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}