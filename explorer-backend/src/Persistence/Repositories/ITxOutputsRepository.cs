using explorer_backend.Models.Data;

namespace explorer_backend.Persistence.Repositories;

public interface ITxOutputsRepository
{
    Task<Guid?> InsertTxOutputAsync(TxOutput txOutputTemplate);
}