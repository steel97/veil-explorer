using ExplorerBackend.Models.Data;
using ExplorerBackend.Models.API;

namespace ExplorerBackend.Persistence.Repositories;

public interface IBlocksRepository
{
    public Task<List<SimplifiedBlock>> GetSimplifiedBlocksAsync(int offset, int count, SortDirection sort, CancellationToken cancellationToken = default);
    public Task<Block?> GetLatestBlockAsync(bool onlySynced = false, CancellationToken cancellationToken = default);
    public Task<Block?> GetBlockAsync(int height, CancellationToken cancellationToken = default);
    public Task<string?> ProbeHashByHeightAsync(int height, CancellationToken cancellationToken = default);
    public Task<int?> ProbeBlockByHashAsync(string hash, CancellationToken cancellationToken = default);
    public Task<Block?> GetBlockAsync(string hash, CancellationToken cancellationToken = default);
    public Task<bool> InsertBlockAsync(Block blockTemplate, CancellationToken cancellationToken = default);
    public Task<bool> UpdateBlockAsync(int height, Block blockTemplate, CancellationToken cancellationToken = default);
    public Task<bool> SetBlockSyncStateAsync(int height, bool state, CancellationToken cancellationToken = default);
}