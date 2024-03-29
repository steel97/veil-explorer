namespace ExplorerBackend.Persistence.Repositories;

public interface IRawTxsRepository
{
    public Task<byte[]?> GetTransactionByIdAsync(string txid, CancellationToken cancellationToken = default);
    public Task<Dictionary<string, byte[]>?> GetTransactionsByIdsAsync(List<string> txids, CancellationToken cancellationToken = default);
    public Task<bool> InsertTransactionAsync(string txid_hex, string txdata_hex, CancellationToken cancellationToken = default);
}