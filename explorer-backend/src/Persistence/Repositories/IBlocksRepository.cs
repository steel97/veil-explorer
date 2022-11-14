using ExplorerBackend.Models.Data;
using ExplorerBackend.Models.API;

namespace ExplorerBackend.Persistence.Repositories;

public interface IBlocksRepository
{
    Task<List<SimplifiedBlock>> GetSimplifiedBlocksAsync(int offset, int count, SortDirection sort, CancellationToken cancellationToken = default);
    Task<Block?> GetLatestBlockAsync(bool onlySynced = false, CancellationToken cancellationToken = default);
    Task<Block?> GetBlockByHeightAsync(int height, CancellationToken cancellationToken = default);
    Task<string?> ProbeHashByHeightAsync(int height, CancellationToken cancellationToken = default);
    Task<int?> ProbeBlockByHashAsync(string hash, CancellationToken cancellationToken = default);
    Task<Block?> GetBlockByHashAsync(string hash, CancellationToken cancellationToken = default);
    Task<bool> InsertBlockAsync(Block blockTemplate, CancellationToken cancellationToken = default);
    Task<bool> UpdateBlockAsync(int height, Block blockTemplate, CancellationToken cancellationToken = default);
    Task<bool> SetBlockSyncStateAsync(int height, bool state, CancellationToken cancellationToken = default);
}