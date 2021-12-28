using Npgsql;
using ExplorerBackend.Services.Core;

namespace ExplorerBackend.Persistence.Repositories;

public class RawTxsRepository : BaseRepository, IRawTxsRepository
{
    public RawTxsRepository(IConfiguration config, IUtilityService utilityService) : base(config, utilityService) { }

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

    public async Task<Dictionary<string, byte[]>?> GetTransactionsByIdsAsync(List<string> txids)
    {
        using var conn = Connection;
        await conn.OpenAsync();

        var query = $"txid = {TransformHex(txids[0])}";
        txids.Skip(1).ToList().ForEach(txid => query += $" OR txid = {TransformHex(txid)}");

        var result = new Dictionary<string, byte[]>();
        using (var cmd = new NpgsqlCommand($"SELECT txid, \"data\" FROM rawtxs WHERE {query}", conn))
        {
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var txid = await ReadHexFromBytea(reader, 0);
                    var data = await ReadBytea(reader, 1);
                    result.Add(txid ?? "", data ?? new byte[] { });
                }
            }
        }
        return result;
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