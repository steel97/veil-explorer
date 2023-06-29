namespace ExplorerBackend.Services.Data;

public class RealtimeRawTransactionsDataService : IRawTransactionsDataService
{
    public Task<byte[]?> GetTransactionByIdAsync(string txid, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Dictionary<string, byte[]>?> GetTransactionsByIdsAsync(List<string> txids, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}