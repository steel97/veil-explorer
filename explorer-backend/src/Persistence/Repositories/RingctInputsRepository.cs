using Npgsql;
using explorer_backend.Models.Data;

namespace explorer_backend.Persistence.Repositories;

public class RingctInputsRepository : BaseRepository, IRingctInputsRepository
{
    public RingctInputsRepository(IConfiguration config) : base(config) { }

    public async Task<Guid?> InsertRingctInputAsync(RingctInput ringctInputTemplate)
    {
        using var conn = Connection;
        await conn.OpenAsync();

        using (var cmd = new NpgsqlCommand(@"INSERT INTO ringct_inputs (tx_input_id,txid,vout_n) VALUES (" +
                                            $"'{ringctInputTemplate.tx_input_id}', {TransformHex(ringctInputTemplate.txid_hex)}, {ringctInputTemplate.vout_n}) RETURNING id;", conn))
        {
            await cmd.PrepareAsync();
            return (Guid?)await cmd.ExecuteScalarAsync();
        }
    }
}