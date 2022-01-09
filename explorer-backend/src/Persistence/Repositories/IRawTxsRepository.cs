namespace ExplorerBackend.Persistence.Repositories;

public interface IRawTxsRepository
{
    Task<byte[]?> GetTransactionByIdAsync(string txid, CancellationToken cancellationToken = default(CancellationToken));
    Task<Dictionary<string, byte[]>?> GetTransactionsByIdsAsync(List<string> txids, CancellationToken cancellationToken = default(CancellationToken));
    Task<bool> InsertTransactionAsync(string txid_hex, string txdata_hex, CancellationToken cancellationToken = default(CancellationToken));
}