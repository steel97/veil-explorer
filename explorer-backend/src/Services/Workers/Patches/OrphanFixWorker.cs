using Microsoft.Extensions.Options;
using ExplorerBackend.Configs;
using ExplorerBackend.Persistence.Repositories;
using ExplorerBackend.Services.Core;

namespace ExplorerBackend.Services.Workers.Patches;

public class OrphanFixWorker(ILogger<OrphanFixWorker> logger, IServiceProvider serviceProvider, IOptionsMonitor<ExplorerConfig> explorerConfig,
    IHttpClientFactory httpClientFactory, IBlocksService blocksService, NodeRequester nodeRequester) : BackgroundService
{
    public static int StartingBlock { get; set; }
    private readonly ILogger _logger = logger;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly IOptionsMonitor<ExplorerConfig> _explorerConfig = explorerConfig;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IBlocksService _blocksService = blocksService;
    private readonly NodeRequester _nodeRequester = nodeRequester;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting orphan checking process from block {blockHeight}", StartingBlock);
        try
        {
            using var scope = _serviceProvider.CreateAsyncScope();

            var blocksRepository = scope.ServiceProvider.GetRequiredService<IBlocksRepository>();
            var transactionsRepository = scope.ServiceProvider.GetRequiredService<ITransactionsRepository>();
            var rawTxsRepository = scope.ServiceProvider.GetRequiredService<IRawTxsRepository>();

        latestBlockPull:
            var latestSyncedBlock = await blocksRepository.GetLatestBlockAsync(true, cancellationToken);
            var currentIndexedBlock = (latestSyncedBlock != null ? latestSyncedBlock.height : 0) + 1;
            if (latestSyncedBlock == null)
            {
                await Task.Delay(1000, cancellationToken);
                goto latestBlockPull;
            }

            _logger.LogInformation("Scanning from {fromBlockHeight}, to {toBlockHeight}", StartingBlock, currentIndexedBlock);

            var foundOrphans = 0;
            var fixedOrphans = 0;
            for (var i = StartingBlock; i < currentIndexedBlock; i++)
            {
                // get hash by height
                var blockHashCheck = await _nodeRequester.GetBlock((uint)i, cancellationToken);

                if (blockHashCheck == null || blockHashCheck.Result == null)
                {
                    //_logger.LogInformation("Can't pull block hash");
                    continue;
                }

                // better to get blocks from db in single query, however this is not "hot path" so...
                var blockFromDB = await blocksRepository.GetBlockAsync(i, cancellationToken);
                if (blockFromDB?.hash_hex == blockHashCheck.Result.Hash)
                    continue;

                foundOrphans++;

                if (!await _blocksService.UpdateDbBlockAsync(i, blockHashCheck.Result.Hash!, cancellationToken))
                {
                    _logger.LogError("Can't update orphan block #{blockNumber}", i);
                }
                else
                {
                    _logger.LogInformation("Update block {blockHeight}, from {fromHash}, to {toHash}", i, blockFromDB?.hash_hex ?? "NULL", blockHashCheck.Result.Hash);
                    fixedOrphans++;
                }
            }

            _logger.LogInformation("Found {totalOrphans}, fixed {fixedOrphans}", foundOrphans, fixedOrphans);

        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Orphan fix failed");
        }
    }
}