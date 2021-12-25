using Npgsql;
using explorer_backend.Models.Data;
using explorer_backend.Models.API;
using explorer_backend.Services.Core;
using explorer_backend.Services.Caching;

namespace explorer_backend.Persistence.Repositories;

public class BlocksRepository : BaseRepository, IBlocksRepository
{
    private readonly ChaininfoSingleton _chainInfoSingleton;

    public BlocksRepository(IConfiguration config, IUtilityService utilityService, ChaininfoSingleton chainInfoSingleton) : base(config, utilityService)
    {
        _chainInfoSingleton = chainInfoSingleton;
    }

    protected async Task<Block?> ReadBlock(NpgsqlDataReader reader)
    {
        var block = new Block();

        block.height = reader.GetInt32(0);
        block.hash_hex = await ReadHexFromBytea(reader, 1);
        block.strippedsize = reader.GetInt32(2);
        block.size = reader.GetInt32(3);
        block.weight = reader.GetInt32(4);
        block.proof_type = (BlockType)reader.GetInt16(5);
        block.proofofstakehash_hex = await ReadHexFromBytea(reader, 6);
        block.progproofofworkhash_hex = await ReadHexFromBytea(reader, 7);
        block.progpowmixhash_hex = await ReadHexFromBytea(reader, 8);
        block.randomxproofofworkhash_hex = await ReadHexFromBytea(reader, 9);
        block.sha256dproofofworkhash_hex = await ReadHexFromBytea(reader, 10);
        block.proofofworkhash_hex = await ReadHexFromBytea(reader, 11);
        block.version = reader.GetInt32(12);
        block.merkleroot_hex = await ReadHexFromBytea(reader, 13);
        block.time = reader.GetInt64(14);
        block.mediantime = reader.GetInt64(15);
        block.nonce = (ulong)reader.GetDecimal(16);
        block.nonce64 = (ulong)reader.GetDecimal(17);
        block.mixhash_hex = await ReadHexFromBytea(reader, 18);
        block.bits_hex = await ReadHexFromBytea(reader, 19);
        block.difficulty = await ReadDoubleFromBytea(reader, 20);
        block.chainwork_hex = await ReadHexFromBytea(reader, 21);
        block.anon_index = reader.GetInt64(22);
        block.veil_data_hash_hex = await ReadHexFromBytea(reader, 23);
        block.prog_header_hash_hex = await ReadHexFromBytea(reader, 24);
        block.prog_header_hex = await ReadHexFromBytea(reader, 25);
        block.epoch_number = reader.GetInt32(26);
        block.synced = reader.GetBoolean(27);

        return block;
    }

    public async Task<List<SimplifiedBlock>> GetSimplifiedBlocks(int offset, int count, SortDirection sort)
    {
        using var conn = Connection;
        await conn.OpenAsync();

        //using (var cmd = new NpgsqlCommand($@"SELECT blocks.height, blocks.""size"", blocks.weight, blocks.proof_type, blocks.""time"", blocks.mediantime, COUNT(transactions.block_height) FROM blocks LEFT JOIN transactions ON blocks.height = transactions.block_height WHERE blocks.synced = true GROUP BY blocks.height ORDER BY height {(sort == SortDirection.ASC ? "ASC" : "DESC")} OFFSET {offset} LIMIT {count};", conn))
        /*
        
        b.height > 
        
        */
        var offsetQuery = sort == SortDirection.DESC ? $"b.height < {_chainInfoSingleton.currentSyncedBlock + 1 - offset}" : $"b.height > {offset}";

        using (var cmd = new NpgsqlCommand($@"SELECT b.height, b.""size"", b.weight, b.proof_type, b.""time"", b.mediantime, (SELECT COUNT(t.txid) as txn from transactions t where t.block_height = b.height) FROM blocks b WHERE {offsetQuery} AND b.synced = true ORDER BY b.height {(sort == SortDirection.ASC ? "ASC" : "DESC")} limit {count};", conn))
        {
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                var simplifiedBlocks = new List<SimplifiedBlock>();

                while (await reader.ReadAsync())
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
        }
    }

    public async Task<Block?> GetLatestBlockAsync(bool onlySynced = false)
    {
        using var conn = Connection;
        await conn.OpenAsync();

        var whereAddition = onlySynced ? "WHERE synced = 'true'" : "";
        using (var cmd = new NpgsqlCommand($"SELECT * FROM blocks {whereAddition} ORDER BY height DESC LIMIT 1;", conn))
        {
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                var success = await reader.ReadAsync();
                if (!success) return null;

                return await ReadBlock(reader);
            }
        }
    }

    public async Task<Block?> GetBlockByHeightAsync(int height)
    {
        using var conn = Connection;
        await conn.OpenAsync();

        using (var cmd = new NpgsqlCommand($"SELECT * FROM blocks WHERE height = {height}", conn))
        {
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                var success = await reader.ReadAsync();
                if (!success) return null;

                return await ReadBlock(reader);
            }
        }
    }

    public async Task<int?> ProbeBlockByHashAsync(string hash)
    {
        using var conn = Connection;
        await conn.OpenAsync();

        using (var cmd = new NpgsqlCommand($"SELECT height FROM blocks WHERE hash = {TransformHex(hash)}", conn))
        {
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                var success = await reader.ReadAsync();
                if (!success) return null;

                return reader.GetInt32(0);
            }
        }
    }

    public async Task<bool> InsertBlockAsync(Block blockTemplate)
    {
        using var conn = Connection;
        await conn.OpenAsync();

        using (var cmd = new NpgsqlCommand(@"INSERT INTO blocks (height,hash,strippedsize,""size"",weight,proof_type,proofofstakehash,progproofofworkhash,progpowmixhash," +
                                            @"randomxproofofworkhash,sha256dproofofworkhash,proofofworkhash,""version"",merkleroot,""time"",mediantime,nonce,nonce64,mixhash," +
                                            @"bits,difficulty,chainwork,anon_index,veil_data_hash,prog_header_hash,prog_header_hex,epoch_number,synced) VALUES (" +
                                            $"{blockTemplate.height}, {TransformHex(blockTemplate.hash_hex)}, {blockTemplate.strippedsize}, {blockTemplate.size}, {blockTemplate.weight}, {(short)blockTemplate.proof_type}, {TransformHex(blockTemplate.proofofstakehash_hex)}, {TransformHex(blockTemplate.progproofofworkhash_hex)}, {TransformHex(blockTemplate.progpowmixhash_hex)}," +
                                            $"{TransformHex(blockTemplate.randomxproofofworkhash_hex)}, {TransformHex(blockTemplate.sha256dproofofworkhash_hex)}, {TransformHex(blockTemplate.proofofworkhash_hex)}, {blockTemplate.version}, {TransformHex(blockTemplate.merkleroot_hex)}, {blockTemplate.time}, {blockTemplate.mediantime}, {blockTemplate.nonce}, {blockTemplate.nonce64}, {TransformHex(blockTemplate.mixhash_hex)}," +
                                            $"{TransformHex(blockTemplate.bits_hex)}, {TransformDouble(blockTemplate.difficulty)}, {TransformHex(blockTemplate.chainwork_hex)}, {blockTemplate.anon_index}, {TransformHex(blockTemplate.veil_data_hash_hex)}, {TransformHex(blockTemplate.prog_header_hash_hex)}, {TransformHex(blockTemplate.prog_header_hex)}, {blockTemplate.epoch_number}, 'false'" +
                                            ");", conn))
        {
            await cmd.PrepareAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }
    }

    public async Task<bool> SetBlockSyncStateAsync(int height, bool state)
    {
        using var conn = Connection;
        await conn.OpenAsync();

        using (var cmd = new NpgsqlCommand($"UPDATE blocks SET synced='true' WHERE height={height};", conn))
        {
            await cmd.PrepareAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }
    }
}