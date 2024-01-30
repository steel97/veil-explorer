using ExplorerBackend.Models.Data;
using ExplorerBackend.Models.Node.Response;

namespace ExplorerBackend.Services;

public interface IBlocksService
{
    public Block RPCBlockToDb(GetBlockResult block, bool isSynced = false);
    public Task<bool> UpdateDbBlockAsync(int height, string validBlockHash, CancellationToken cancellationToken);
    public Task<bool> InsertTransactionsAsync(int blockId, List<string>? txIds, CancellationToken cancellationToken);
    public Task<bool> InsertTransactionsAsync(int height, List<GetRawTransactionResult> txs, CancellationToken cancellationToken);
    public BlockType GetBlockType(string proofType);
}