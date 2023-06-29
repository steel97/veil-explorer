using ExplorerBackend.Models.API;
using ExplorerBackend.Models.Data;

namespace ExplorerBackend.Services.Data;

public class RealtimeBlocksDataService : IBlocksDataService
{
    public Task<Block?> GetBlockByHashAsync(string hash, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Block?> GetBlockByHeightAsync(int height, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Block?> GetLatestBlockAsync(bool onlySynced = false, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<List<SimplifiedBlock>> GetSimplifiedBlocksAsync(int offset, int count, SortDirection sort, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<int?> ProbeBlockByHashAsync(string hash, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<string?> ProbeHashByHeightAsync(int height, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}