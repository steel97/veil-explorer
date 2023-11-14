using ExplorerBackend.Models.API;
using ExplorerBackend.Models.Data;

namespace ExplorerBackend.Services.Data;

public interface IBlocksDataService
{
    public Task<Block?> GetLatestBlockAsync(bool onlySynced = false, CancellationToken cancellationToken = default);
    public Task<Block?> GetBlockAsync(string hash, CancellationToken cancellationToken = default);
    public Task<Block?> GetBlockAsync(int height, CancellationToken cancellationToken = default);
    public Task<List<SimplifiedBlock>> GetSimplifiedBlocksAsync(int offset, int count, SortDirection sort, CancellationToken cancellationToken = default);
    public Task<string?> ProbeHashByHeightAsync(int height, CancellationToken cancellationToken = default);
    public Task<int?> ProbeBlockByHashAsync(string hash, CancellationToken cancellationToken = default);
}