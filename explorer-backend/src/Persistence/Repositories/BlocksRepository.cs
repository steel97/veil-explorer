using Npgsql;
using ExplorerBackend.Models.Data;
using ExplorerBackend.Models.API;
using ExplorerBackend.Services.Core;
using ExplorerBackend.Services.Caching;

namespace ExplorerBackend.Persistence.Repositories;

public class BlocksRepository : BaseRepository, IBlocksRepository
{
    private readonly ChaininfoSingleton _chainInfoSingleton;

    public BlocksRepository(IConfiguration config, IUtilityService utilityService, ChaininfoSingleton chainInfoSingleton) : base(config, utilityService)
    {
        _chainInfoSingleton = chainInfoSingleton;
    }

    protected async Task<Block?> ReadBlockAsync(NpgsqlDataReader reader, CancellationToken cancellationToken = default)
    {
        var block = new Block
        {
            height = reader.GetInt32(0),
            hash_hex = await ReadHexFromByteaAsync(reader, 1, cancellationToken),
            strippedsize = reader.GetInt32(2),
            size = reader.GetInt32(3),
            weight = reader.GetInt32(4),
            proof_type = (BlockType)reader.GetInt16(5),
            proofofstakehash_hex = await ReadHexFromByteaAsync(reader, 6, cancellationToken),
            progproofofworkhash_hex = await ReadHexFromByteaAsync(reader, 7, cancellationToken),
            progpowmixhash_hex = await ReadHexFromByteaAsync(reader, 8, cancellationToken),
            randomxproofofworkhash_hex = await ReadHexFromByteaAsync(reader, 9, cancellationToken),
            sha256dproofofworkhash_hex = await ReadHexFromByteaAsync(reader, 10, cancellationToken),
            proofofworkhash_hex = await ReadHexFromByteaAsync(reader, 11, cancellationToken),
            version = reader.GetInt32(12),
            merkleroot_hex = await ReadHexFromByteaAsync(reader, 13, cancellationToken),
            time = reader.GetInt64(14),
            mediantime = reader.GetInt64(15),
            nonce = (ulong)reader.GetDecimal(16),
            nonce64 = (ulong)reader.GetDecimal(17),
            mixhash_hex = await ReadHexFromByteaAsync(reader, 18, cancellationToken),
            bits_hex = await ReadHexFromByteaAsync(reader, 19, cancellationToken),
            difficulty = await ReadDoubleFromByteaAsync(reader, 20, cancellationToken),
            chainwork_hex = await ReadHexFromByteaAsync(reader, 21, cancellationToken),
            anon_index = reader.GetInt64(22),
            veil_data_hash_hex = await ReadHexFromByteaAsync(reader, 23, cancellationToken),
            prog_header_hash_hex = await ReadHexFromByteaAsync(reader, 24, cancellationToken),
            prog_header_hex = await ReadHexFromByteaAsync(reader, 25, cancellationToken),
            epoch_number = reader.GetInt32(26),
            synced = reader.GetBoolean(27)
        };

        return block;
    }

    public async Task<List<SimplifiedBlock>> GetSimplifiedBlocksAsync(int offset, int count, SortDirection sort, CancellationToken cancellationToken = default)
    {
        using var conn = Connection;
        await conn.OpenAsync(cancellationToken);

        var offsetQuery = sort == SortDirection.DESC ? $"b.height < {_chainInfoSingleton.CurrentSyncedBlock + 1 - offset}" : $"b.height > {offset}";

        using var cmd = new NpgsqlCommand($"SELECT b.height, b.\"size\", b.weight, b.proof_type, b.\"time\", b.mediantime, (SELECT COUNT(t.txid) as txn from transactions t where t.block_height = b.height) FROM blocks b WHERE {offsetQuery} AND b.synced = true ORDER BY b.height {(sort == SortDirection.ASC ? "ASC" : "DESC")} limit {count};", conn);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var simplifiedBlocks = new List<SimplifiedBlock>();

        while (await reader.ReadAsync(cancellationToken))
        {

            var simplifiedBlock = new SimplifiedBlock
            {
                Height = reader.GetInt32(0),
                Size = reader.GetInt32(1),
                Weight = reader.GetInt32(2),
                ProofType = (BlockType)reader.GetInt16(3),
                Time = reader.GetInt64(4),
                MedianTime = reader.GetInt64(5),
                TxCount = reader.GetInt32(6)
            };
            simplifiedBlocks.Add(simplifiedBlock);
        }

        return simplifiedBlocks;
    }

    public async Task<Block?> GetLatestBlockAsync(bool onlySynced = false, CancellationToken cancellationToken = default)
    {
        using var conn = Connection;
        await conn.OpenAsync(cancellationToken);

        var whereAddition = onlySynced ? "WHERE synced = 'true'" : "";
        using var cmd = new NpgsqlCommand($"SELECT * FROM blocks {whereAddition} ORDER BY height DESC LIMIT 1;", conn);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var success = await reader.ReadAsync(cancellationToken);
        if (!success) return null;

        return await ReadBlockAsync(reader, cancellationToken);
    }

    public async Task<Block?> GetBlockByHeightAsync(int height, CancellationToken cancellationToken = default)
    {
        using var conn = Connection;
        await conn.OpenAsync(cancellationToken);

        using var cmd = new NpgsqlCommand($"SELECT b.height, b.hash, b.strippedsize, b.\"size\", b.weight, b.proof_type, b.proofofstakehash , b.progproofofworkhash, b.progpowmixhash ,b.randomxproofofworkhash ,b.sha256dproofofworkhash , b.proofofworkhash, b.\"version\", b.merkleroot ,b.\"time\", b.mediantime,b.nonce ,b.nonce64 ,b.mixhash , b.bits ,b.difficulty ,b.chainwork ,b.anon_index ,b.veil_data_hash ,b.prog_header_hash ,b.prog_header_hex , b.epoch_number , b.synced,  (SELECT COUNT(t.txid) as txn from transactions t where t.block_height = b.height) FROM blocks b WHERE b.height = {height};", conn);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var success = await reader.ReadAsync(cancellationToken);
        if (!success) return null;

        var block = await ReadBlockAsync(reader, cancellationToken);
        if (block != null)
            block.txnCount = reader.GetInt32(28);

        return block;
    }

    public async Task<string?> ProbeHashByHeightAsync(int height, CancellationToken cancellationToken = default)
    {
        using var conn = Connection;
        await conn.OpenAsync(cancellationToken);

        using var cmd = new NpgsqlCommand($"SELECT hash FROM blocks WHERE height = {height}", conn);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var success = await reader.ReadAsync(cancellationToken);
        if (!success) return null;

        return await ReadHexFromByteaAsync(reader, 0, cancellationToken);
    }

    public async Task<Block?> GetBlockByHashAsync(string hash, CancellationToken cancellationToken = default)
    {
        using var conn = Connection;
        await conn.OpenAsync(cancellationToken);

        using var cmd = new NpgsqlCommand($"SELECT b.height, b.hash, b.strippedsize, b.\"size\", b.weight, b.proof_type, b.proofofstakehash , b.progproofofworkhash, b.progpowmixhash ,b.randomxproofofworkhash ,b.sha256dproofofworkhash , b.proofofworkhash, b.\"version\", b.merkleroot, b.\"time\", b.mediantime,b.nonce ,b.nonce64 ,b.mixhash , b.bits ,b.difficulty ,b.chainwork ,b.anon_index ,b.veil_data_hash ,b.prog_header_hash ,b.prog_header_hex , b.epoch_number , b.synced,  (SELECT COUNT(t.txid) as txn from transactions t where t.block_height = b.height) FROM blocks b WHERE b.hash = {TransformHex(hash)};", conn);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var success = await reader.ReadAsync(cancellationToken);
        if (!success) return null;

        var block = await ReadBlockAsync(reader, cancellationToken);
        if (block != null)
            block.txnCount = reader.GetInt32(28);
        return block;
    }


    public async Task<int?> ProbeBlockByHashAsync(string hash, CancellationToken cancellationToken = default)
    {
        using var conn = Connection;
        await conn.OpenAsync(cancellationToken);

        using var cmd = new NpgsqlCommand($"SELECT height FROM blocks WHERE hash = {TransformHex(hash)}", conn);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var success = await reader.ReadAsync(cancellationToken);
        if (!success) return null;

        return reader.GetInt32(0);
    }

    public async Task<bool> InsertBlockAsync(Block blockTemplate, CancellationToken cancellationToken = default)
    {
        using var conn = Connection;
        await conn.OpenAsync(cancellationToken);

        using var cmd = new NpgsqlCommand("INSERT INTO blocks (height,hash,strippedsize,\"size\",weight,proof_type,proofofstakehash,progproofofworkhash,progpowmixhash," +
                                            "randomxproofofworkhash,sha256dproofofworkhash,proofofworkhash,\"version\",merkleroot,\"time\",mediantime,nonce,nonce64,mixhash," +
                                            "bits,difficulty,chainwork,anon_index,veil_data_hash,prog_header_hash,prog_header_hex,epoch_number,synced) VALUES (" +
                                            $"{blockTemplate.height}, {TransformHex(blockTemplate.hash_hex)}, {blockTemplate.strippedsize}, {blockTemplate.size}, {blockTemplate.weight}, {(short)blockTemplate.proof_type}, {TransformHex(blockTemplate.proofofstakehash_hex)}, {TransformHex(blockTemplate.progproofofworkhash_hex)}, {TransformHex(blockTemplate.progpowmixhash_hex)}," +
                                            $"{TransformHex(blockTemplate.randomxproofofworkhash_hex)}, {TransformHex(blockTemplate.sha256dproofofworkhash_hex)}, {TransformHex(blockTemplate.proofofworkhash_hex)}, {blockTemplate.version}, {TransformHex(blockTemplate.merkleroot_hex)}, {blockTemplate.time}, {blockTemplate.mediantime}, {blockTemplate.nonce}, {blockTemplate.nonce64}, {TransformHex(blockTemplate.mixhash_hex)}," +
                                            $"{TransformHex(blockTemplate.bits_hex)}, {TransformDouble(blockTemplate.difficulty)}, {TransformHex(blockTemplate.chainwork_hex)}, {blockTemplate.anon_index}, {TransformHex(blockTemplate.veil_data_hash_hex)}, {TransformHex(blockTemplate.prog_header_hash_hex)}, {TransformHex(blockTemplate.prog_header_hex)}, {blockTemplate.epoch_number}, 'false'" +
                                            ");", conn);
        await cmd.PrepareAsync(cancellationToken);
        return await cmd.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<bool> SetBlockSyncStateAsync(int height, bool state, CancellationToken cancellationToken = default)
    {
        using var conn = Connection;
        await conn.OpenAsync(cancellationToken);

        using var cmd = new NpgsqlCommand($"UPDATE blocks SET synced='true' WHERE height={height};", conn);
        await cmd.PrepareAsync(cancellationToken);
        return await cmd.ExecuteNonQueryAsync(cancellationToken) > 0;
    }
}