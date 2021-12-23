using explorer_backend.Models.Data;

namespace explorer_backend.Persistence.Repositories;

public interface ITxInputsRepository
{
    Task<Guid?> InsertTxInputAsync(TxInput txInputTemplate);
}