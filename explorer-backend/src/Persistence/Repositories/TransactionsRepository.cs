using Npgsql;
using ExplorerBackend.Models.Data;
using ExplorerBackend.Services.Core;

namespace ExplorerBackend.Persistence.Repositories;

public class TransactionsRepository : BaseRepository, ITransactionsRepository
{
    private readonly NpgsqlDataSource _dataSource;
    public TransactionsRepository(NpgsqlDataSource dataSource, IUtilityService utilityService) : base(utilityService) => _dataSource = dataSource;

    protected async Task<Transaction?> ReadTransactionAsync(NpgsqlDataReader reader, CancellationToken cancellationToken = default)
    {
        var tx = new Transaction
        {
            txid_hex = await ReadHexFromByteaAsync(reader, 0, cancellationToken),
            hash_hex = await ReadHexFromByteaAsync(reader, 1, cancellationToken),
            version = reader.GetInt32(2),
            size = reader.GetInt32(3),
            vsize = reader.GetInt32(4),
            weight = reader.GetInt32(5),
            locktime = reader.GetInt64(6),
            block_height = reader.GetInt32(7)
        };

        return tx;
    }

    public async Task<Transaction?> GetTransactionByIdAsync(string txid, CancellationToken cancellationToken = default)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand($"SELECT * FROM transactions WHERE txid = {TransformHex(txid)}", conn);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var success = await reader.ReadAsync(cancellationToken);
        if (!success) return null;

        return await ReadTransactionAsync(reader, cancellationToken);
    }


    public async Task<TransactionExtended?> GetTransactionFullByIdAsync(string txid, CancellationToken cancellationToken = default)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand($"SELECT t.txid, t.hash, t.\"version\", t.\"size\", t.vsize, t.weight, t.locktime, t.block_height, r.\"data\" FROM transactions as t INNER JOIN rawtxs r ON t.txid = r.txid  WHERE t.txid = {TransformHex(txid)};", conn);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var success = await reader.ReadAsync(cancellationToken);
        if (!success) return null;
        var rt = await ReadTransactionAsync(reader, cancellationToken);
        if (rt != null)
        {
            var tx = new TransactionExtended
            {
                txid_hex = rt.txid_hex,
                hash_hex = rt.hash_hex,
                version = rt.version,
                size = rt.size,
                vsize = rt.vsize,
                weight = rt.weight,
                locktime = rt.locktime,
                block_height = rt.block_height,
                data = await ReadByteaAsync(reader, 8, cancellationToken)
            };
            return tx;
        }


        return null;
    }

    public async Task<List<TransactionExtended>?> GetTransactionsForBlockAsync(int blockHeight, int offset, int count, bool fetchAll, CancellationToken cancellationToken = default)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);

        var addition = fetchAll ? "" : $" OFFSET {offset} LIMIT {count}";

        await using var cmd = new NpgsqlCommand($"SELECT t.txid, t.hash, t.\"version\", t.\"size\", t.vsize, t.weight, t.locktime, t.block_height, r.\"data\" FROM transactions as t INNER JOIN rawtxs r ON t.txid = r.txid  WHERE t.block_height = {blockHeight}{addition};", conn);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var txs = new List<TransactionExtended>();

        while (await reader.ReadAsync(cancellationToken))
        {
            var rt = await ReadTransactionAsync(reader, cancellationToken);
            if (rt != null)
            {
                var tx = new TransactionExtended
                {
                    txid_hex = rt.txid_hex,
                    hash_hex = rt.hash_hex,
                    version = rt.version,
                    size = rt.size,
                    vsize = rt.vsize,
                    weight = rt.weight,
                    locktime = rt.locktime,
                    block_height = rt.block_height,
                    data = await ReadByteaAsync(reader, 8, cancellationToken)
                };
                txs.Add(tx);
            }
        }

        return txs;
    }

    public async Task<bool> RemoveTransactionsForBlockAsync(int blockHeight, CancellationToken cancellationToken = default)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand($"DELETE FROM transactions WHERE block_height = {blockHeight};", conn);
        return await cmd.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<string?> ProbeTransactionByHashAsync(string txid, CancellationToken cancellationToken = default)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand($"SELECT txid FROM transactions WHERE txid = {TransformHex(txid)}", conn);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var success = await reader.ReadAsync(cancellationToken);
        if (!success) return null;

        return await ReadHexFromByteaAsync(reader, 0, cancellationToken);
    }

    public async Task<bool> InsertTransactionAsync(Transaction txTemplate, CancellationToken cancellationToken = default)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand("INSERT INTO transactions (txid,hash,\"version\",\"size\",vsize,weight,locktime,block_height) VALUES (" +
                                            $"{TransformHex(txTemplate.txid_hex)}, {TransformHex(txTemplate.hash_hex)}, {txTemplate.version}, {txTemplate.size}, {txTemplate.vsize}, {txTemplate.weight}, {txTemplate.locktime}, {txTemplate.block_height});", conn);
        await cmd.PrepareAsync(cancellationToken);
        return await cmd.ExecuteNonQueryAsync(cancellationToken) > 0;
    }
}