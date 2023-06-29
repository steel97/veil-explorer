using ExplorerBackend.Models.API;
using ExplorerBackend.Models.Data;
using ExplorerBackend.Persistence.Repositories;

namespace ExplorerBackend.Services.Data;

public class DbBlocksDataService : IBlocksDataService
{
    private readonly IBlocksRepository _blocksRepository;
    public DbBlocksDataService(IBlocksRepository blocksRepository) => (_blocksRepository) = (blocksRepository);

    Task<Block?> IBlocksDataService.GetBlockByHashAsync(string hash, CancellationToken cancellationToken) =>
        _blocksRepository.GetBlockByHashAsync(hash, cancellationToken);

    Task<Block?> IBlocksDataService.GetBlockByHeightAsync(int height, CancellationToken cancellationToken) =>
        _blocksRepository.GetBlockByHeightAsync(height, cancellationToken);

    Task<Block?> IBlocksDataService.GetLatestBlockAsync(bool onlySynced, CancellationToken cancellationToken) =>
        _blocksRepository.GetLatestBlockAsync(onlySynced, cancellationToken);
    Task<List<SimplifiedBlock>> IBlocksDataService.GetSimplifiedBlocksAsync(int offset, int count, SortDirection sort, CancellationToken cancellationToken) =>
        _blocksRepository.GetSimplifiedBlocksAsync(offset, count, sort, cancellationToken);

    Task<int?> IBlocksDataService.ProbeBlockByHashAsync(string hash, CancellationToken cancellationToken) =>
        _blocksRepository.ProbeBlockByHashAsync(hash, cancellationToken);

    Task<string?> IBlocksDataService.ProbeHashByHeightAsync(int height, CancellationToken cancellationToken) =>
        _blocksRepository.ProbeHashByHeightAsync(height, cancellationToken);
}