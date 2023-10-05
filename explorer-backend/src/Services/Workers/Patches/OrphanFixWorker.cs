using System.Text;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using ExplorerBackend.Configs;
using ExplorerBackend.Persistence.Repositories;
using ExplorerBackend.Services.Core;

namespace ExplorerBackend.Services.Workers.Patches;

public class OrphanFixWorker : BackgroundService
{
    public static int StartingBlock { get; set; }
    private Uri? _uri;
    private AuthenticationHeaderValue? _authHeader;
    private int _usernameHash;
    private int _passHash;
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptionsMonitor<ExplorerConfig> _explorerConfig;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IBlocksService _blocksService;
    private readonly NodeRequester _nodeRequester;

    public OrphanFixWorker(ILogger<OrphanFixWorker> logger, IServiceProvider serviceProvider, IOptionsMonitor<ExplorerConfig> explorerConfig,
        IHttpClientFactory httpClientFactory, IBlocksService blocksService, NodeRequester nodeRequester)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _explorerConfig = explorerConfig;
        _httpClientFactory = httpClientFactory;
        _blocksService = blocksService;
        _nodeRequester = nodeRequester;
        ConfigSetup();
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var httpClient = _httpClientFactory.CreateClient();

       if(_passHash !=_explorerConfig.CurrentValue.Node!.Password!.GetHashCode() || _usernameHash !=_explorerConfig.CurrentValue.Node!.Username!.GetHashCode())        
            ConfigSetup();
                    
        httpClient.BaseAddress = _uri;
        httpClient.DefaultRequestHeaders.Authorization = _authHeader;

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
                var blockHashCheck = await _nodeRequester.GetBlockHash((uint)i, httpClient, cancellationToken);

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

                if (!await _blocksService.UpdateDbBlockAsync(i, blockHashCheck.Result, cancellationToken))
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
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Orphan fix failed");
        }
    }
    private void ConfigSetup()
    {
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node);
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node.Url);
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node.Username);
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node.Password);

        _authHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_explorerConfig.CurrentValue.Node!.Username}:{_explorerConfig.CurrentValue.Node.Password}")));
        _uri = new Uri(_explorerConfig.CurrentValue.Node!.Url!);
        _usernameHash = _explorerConfig.CurrentValue.Node.Password!.GetHashCode();
        _passHash = _explorerConfig.CurrentValue.Node!.Username!.GetHashCode();
    }
}