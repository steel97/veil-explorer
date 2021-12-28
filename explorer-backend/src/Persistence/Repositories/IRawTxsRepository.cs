namespace ExplorerBackend.Persistence.Repositories;

public interface IRawTxsRepository
{
    Task<byte[]?> GetTransactionByIdAsync(string txid);
    Task<Dictionary<string, byte[]>?> GetTransactionsByIdsAsync(List<string> txids);
    Task<bool> InsertTransactionAsync(string txid_hex, string txdata_hex);
}