namespace ExplorerBackend.Services.Data;

public interface IRawTransactionsDataService
{
    public Task<byte[]?> GetTransactionByIdAsync(string txid, CancellationToken cancellationToken = default);
    public Task<Dictionary<string, byte[]>?> GetTransactionsByIdsAsync(List<string> txids, CancellationToken cancellationToken = default);
}