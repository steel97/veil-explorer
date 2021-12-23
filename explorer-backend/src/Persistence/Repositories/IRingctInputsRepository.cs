using explorer_backend.Models.Data;

namespace explorer_backend.Persistence.Repositories;

public interface IRingctInputsRepository
{
    Task<Guid?> InsertRingctInputAsync(RingctInput ringctInputTemplate);
}