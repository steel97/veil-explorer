using Npgsql;

namespace explorer_backend.Persistence.Repositories;

public class RawTxsRepository : BaseRepository, IRawTxsRepository
{
    public RawTxsRepository(IConfiguration config) : base(config) { }

    public async Task<byte[]?> GetTransactionByIdAsync(string txid)
    {
        using var conn = Connection;
        await conn.OpenAsync();

        using (var cmd = new NpgsqlCommand($"SELECT \"data\" FROM rawtxs WHERE txid = {TransformHex(txid)}", conn))
        {
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                var success = await reader.ReadAsync();
                if (!success) return null;

                return await ReadBytea(reader, 0);
            }
        }
    }

    public async Task<bool> InsertTransactionAsync(string txid_hex, string data_hex)
    {
        using var conn = Connection;
        await conn.OpenAsync();

        using (var cmd = new NpgsqlCommand(@$"INSERT INTO rawtxs (txid,""data"") VALUES ({TransformHex(txid_hex)}, {TransformHex(data_hex)});", conn))
        {
            await cmd.PrepareAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }
    }
}