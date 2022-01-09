using Npgsql;
using ExplorerBackend.Services.Core;

namespace ExplorerBackend.Persistence.Repositories;

public class RawTxsRepository : BaseRepository, IRawTxsRepository
{
    public RawTxsRepository(IConfiguration config, IUtilityService utilityService) : base(config, utilityService) { }

    public async Task<byte[]?> GetTransactionByIdAsync(string txid, CancellationToken cancellationToken = default(CancellationToken))
    {
        using var conn = Connection;
        await conn.OpenAsync(cancellationToken);

        using (var cmd = new NpgsqlCommand($"SELECT \"data\" FROM rawtxs WHERE txid = {TransformHex(txid)}", conn))
        {
            await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
            {
                var success = await reader.ReadAsync(cancellationToken);
                if (!success) return null;

                return await ReadByteaAsync(reader, 0, cancellationToken);
            }
        }
    }

    public async Task<Dictionary<string, byte[]>?> GetTransactionsByIdsAsync(List<string> txids, CancellationToken cancellationToken = default(CancellationToken))
    {
        using var conn = Connection;
        await conn.OpenAsync(cancellationToken);

        var query = $"txid = {TransformHex(txids[0])}";
        txids.Skip(1).ToList().ForEach(txid => query += $" OR txid = {TransformHex(txid)}");

        var result = new Dictionary<string, byte[]>();
        using (var cmd = new NpgsqlCommand($"SELECT txid, \"data\" FROM rawtxs WHERE {query}", conn))
        {
            await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
            {
                while (await reader.ReadAsync())
                {
                    var txid = await ReadHexFromByteaAsync(reader, 0, cancellationToken);
                    var data = await ReadByteaAsync(reader, 1, cancellationToken);
                    result.Add(txid ?? "", data ?? new byte[] { });
                }
            }
        }
        return result;
    }

    public async Task<bool> InsertTransactionAsync(string txid_hex, string data_hex, CancellationToken cancellationToken = default(CancellationToken))
    {
        using var conn = Connection;
        await conn.OpenAsync(cancellationToken);

        using (var cmd = new NpgsqlCommand($"INSERT INTO rawtxs (txid,\"data\") VALUES ({TransformHex(txid_hex)}, {TransformHex(data_hex)});", conn))
        {
            await cmd.PrepareAsync(cancellationToken);
            return await cmd.ExecuteNonQueryAsync(cancellationToken) > 0;
        }
    }
}