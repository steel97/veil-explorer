using ExplorerBackend.Models.Data;
using ExplorerBackend.Models.API;

namespace ExplorerBackend.Persistence.Repositories;

public interface IBlocksRepository
{
    Task<List<SimplifiedBlock>> GetSimplifiedBlocksAsync(int offset, int count, SortDirection sort, CancellationToken cancellationToken = default(CancellationToken));
    Task<Block?> GetLatestBlockAsync(bool onlySynced = false, CancellationToken cancellationToken = default(CancellationToken));
    Task<Block?> GetBlockByHeightAsync(int height, CancellationToken cancellationToken = default(CancellationToken));
    Task<string?> ProbeHashByHeightAsync(int height, CancellationToken cancellationToken = default(CancellationToken));
    Task<int?> ProbeBlockByHashAsync(string hash, CancellationToken cancellationToken = default(CancellationToken));
    Task<Block?> GetBlockByHashAsync(string hash, CancellationToken cancellationToken = default(CancellationToken));
    Task<bool> InsertBlockAsync(Block blockTemplate, CancellationToken cancellationToken = default(CancellationToken));
    Task<bool> SetBlockSyncStateAsync(int height, bool state, CancellationToken cancellationToken = default(CancellationToken));
}