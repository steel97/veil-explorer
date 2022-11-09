namespace ExplorerBackend.Persistence.Repositories;

public interface IRawTxsRepository
{
    Task<byte[]?> GetTransactionByIdAsync(string txid, CancellationToken cancellationToken = default);
    Task<Dictionary<string, byte[]>?> GetTransactionsByIdsAsync(List<string> txids, CancellationToken cancellationToken = default);
    Task<bool> InsertTransactionAsync(string txid_hex, string txdata_hex, CancellationToken cancellationToken = default);
}