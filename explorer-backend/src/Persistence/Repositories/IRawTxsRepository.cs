using explorer_backend.Models.Data;

namespace explorer_backend.Persistence.Repositories;

public interface IRawTxsRepository
{
    Task<byte[]?> GetTransactionByIdAsync(string txid);
    Task<bool> InsertTransactionAsync(string txid_hex, string txdata_hex);
}