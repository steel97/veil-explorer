using explorer_backend.Models.Data;
using explorer_backend.Models.API;

namespace explorer_backend.Persistence.Repositories;

public interface IBlocksRepository
{
    Task<List<SimplifiedBlock>> GetSimplifiedBlocks(int offset, int count, SortDirection sort);
    Task<Block?> GetLatestBlockAsync(bool onlySynced = false);
    Task<Block?> GetBlockByHeightAsync(int height);
    Task<bool> InsertBlockAsync(Block blockTemplate);
    Task<bool> SetBlockSyncStateAsync(int height, bool state);
}