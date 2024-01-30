using ExplorerBackend.Models.API;
using ExplorerBackend.Models.Data;
using ExplorerBackend.Persistence.Repositories;

namespace ExplorerBackend.Services.Data;

public class DbBlocksDataService : IBlocksDataService
{
    private readonly IBlocksRepository _blocksRepository;
    public DbBlocksDataService(IBlocksRepository blocksRepository) => _blocksRepository = blocksRepository;

    public Task<Block?> GetBlockAsync(string hash, int simplifiedTxInfo, CancellationToken cancellationToken) =>
        _blocksRepository.GetBlockAsync(hash, cancellationToken);

    public Task<Block?> GetBlockAsync(int height, int simplifiedTxInfo, CancellationToken cancellationToken) =>
        _blocksRepository.GetBlockAsync(height, cancellationToken);

    public Task<Block?> GetLatestBlockAsync(bool onlySynced, CancellationToken cancellationToken) =>
        _blocksRepository.GetLatestBlockAsync(onlySynced, cancellationToken);
    public Task<List<SimplifiedBlock>> GetSimplifiedBlocksAsync(int offset, int count, SortDirection sort, CancellationToken cancellationToken) =>
        _blocksRepository.GetSimplifiedBlocksAsync(offset, count, sort, cancellationToken);

    public Task<int?> ProbeBlockByHashAsync(string hash, CancellationToken cancellationToken) =>
        _blocksRepository.ProbeBlockByHashAsync(hash, cancellationToken);

    public Task<string?> ProbeHashByHeightAsync(int height, CancellationToken cancellationToken) =>
        _blocksRepository.ProbeHashByHeightAsync(height, cancellationToken);
}