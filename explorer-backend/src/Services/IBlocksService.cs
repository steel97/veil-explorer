using ExplorerBackend.Models.Data;
using ExplorerBackend.Models.Node.Response;

namespace ExplorerBackend.Services;

public interface IBlocksService
{
    public Block RPCBlockToDb(GetBlockResult block, bool isSynced = false);
    Task<bool> UpdateDbBlockAsync(int height, string validBlockHash, CancellationToken cancellationToken);
    Task<bool> InsertTransactionsAsync(int blockId, List<string>? txIds, CancellationToken cancellationToken);
    Task<bool> InsertTransactionsAsync(int height, List<GetRawTransactionResult> txs, CancellationToken cancellationToken);
}