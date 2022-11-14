using System.Text;
using System.Text.Json;
using System.Transactions;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.SignalR;
using ExplorerBackend.Hubs;
using ExplorerBackend.Configs;
using ExplorerBackend.Services.Caching;
using ExplorerBackend.Persistence.Repositories;
using ExplorerBackend.Models.API;
using ExplorerBackend.Models.Data;
using ExplorerBackend.Models.Node;
using ExplorerBackend.Models.Node.Response;

namespace ExplorerBackend.Services.Workers.Patches;

public class OrphanFixWorker : BackgroundService
{
    public static int StartingBlock { get; set; }

    private readonly ILogger _logger;
    private readonly IHubContext<EventsHub> _hubContext;
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptionsMonitor<ExplorerConfig> _explorerConfig;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ChaininfoSingleton _chainInfoSingleton;
    private readonly IBlocksService _blocksService;

    public OrphanFixWorker(ILogger<OrphanFixWorker> logger, IHubContext<EventsHub> hubContext,
        IServiceProvider serviceProvider, IOptionsMonitor<ExplorerConfig> explorerConfig,
        IHttpClientFactory httpClientFactory, ChaininfoSingleton chaininfoSingleton,
        IBlocksService blocksService)
    {
        _logger = logger;
        _hubContext = hubContext;
        _serviceProvider = serviceProvider;
        _explorerConfig = explorerConfig;
        _httpClientFactory = httpClientFactory;
        _chainInfoSingleton = chaininfoSingleton;
        _blocksService = blocksService;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var httpClient = _httpClientFactory.CreateClient();

        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node);
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node.Url);
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node.Username);
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node.Password);

        httpClient.BaseAddress = new Uri(_explorerConfig.CurrentValue.Node.Url);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_explorerConfig.CurrentValue.Node.Username}:{_explorerConfig.CurrentValue.Node.Password}")));

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

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
                var getBlockHashCheckRequest = new JsonRPCRequest
                {
                    Id = 1,
                    Method = "getblockhash",
                    Params = new List<object>(new object[] { i })
                };
                var getBlockHashCheckResponse = await httpClient.PostAsJsonAsync("", getBlockHashCheckRequest, options, cancellationToken);
                var blockHashCheck = await getBlockHashCheckResponse.Content.ReadFromJsonAsync<GetBlockHash>(options, cancellationToken);

                if (blockHashCheck == null || blockHashCheck.Result == null)
                {
                    //_logger.LogInformation("Can't pull block hash");
                    continue;
                }

                // better to get blocks from db in single query, however this is not "hot path" so...
                var blockFromDB = await blocksRepository.GetBlockByHeightAsync(i, cancellationToken);
                if (blockFromDB?.hash_hex == blockHashCheck.Result)
                    continue;

                foundOrphans++;

                if (await _blocksService.UpdateDbBlockAsync(i, blockHashCheck.Result, cancellationToken)) // true = txfailed
                {
                    _logger.LogError("Can't update orphan block #{blockNumber}", i);
                }
                else
                {
                    _logger.LogInformation("Update block {blockHeight}, from {fromHash}, to {toHash}", i, blockFromDB?.hash_hex ?? "NULL", blockHashCheck.Result);
                    fixedOrphans++;
                }
            }

            _logger.LogInformation("Found {totalOrphans}, fixed {fixedOrphans}", foundOrphans, fixedOrphans);

        }
        catch (OperationCanceledException)
        {

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Orphan fix failed");
        }
    }
}