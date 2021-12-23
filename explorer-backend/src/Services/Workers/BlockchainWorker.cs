using System.Text.Json;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using explorer_backend.Configs;
using explorer_backend.Services.Caching;
using explorer_backend.Models.Node;
using explorer_backend.Models.Node.Response;

namespace explorer_backend.Services.Workers;

public class BlockchainWorker : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IOptionsMonitor<ExplorerConfig> _explorerConfig;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ChaininfoSingleton _chainInfoSingleton;

    public BlockchainWorker(ILogger<BlockchainWorker> logger, IOptionsMonitor<ExplorerConfig> explorerConfig, IHttpClientFactory httpClientFactory, ChaininfoSingleton chaininfoSingleton)
    {
        _logger = logger;
        _explorerConfig = explorerConfig;
        _httpClientFactory = httpClientFactory;
        _chainInfoSingleton = chaininfoSingleton;
    }
    protected override async Task ExecuteAsync(CancellationToken stopToken)
    {
        using var httpClient = _httpClientFactory.CreateClient();

        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node);
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node.Url);
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node.Authorization);

        httpClient.BaseAddress = new Uri(_explorerConfig.CurrentValue.Node.Url);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _explorerConfig.CurrentValue.Node.Authorization);

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        while (!stopToken.IsCancellationRequested)
        {
            try
            {
                // get blockchain info
                var getBlockchainInfoRequest = new JsonRPCRequest
                {
                    Id = 1,
                    Method = "getblockchaininfo",
                    Params = new List<object>(new object[] { })
                };
                var getBlockchainInfoResponse = await httpClient.PostAsJsonAsync<JsonRPCRequest>("", getBlockchainInfoRequest, options);
                var blockchainInfo = await getBlockchainInfoResponse.Content.ReadFromJsonAsync<GetBlockchainInfo>(options);

                // get chainalgo stats
                var getChainalgoStatsRequest = new JsonRPCRequest
                {
                    Id = 1,
                    Method = "getchainalgostats",
                    Params = new List<object>(new object[] { })
                };
                var getChainalgoStatsResponse = await httpClient.PostAsJsonAsync<JsonRPCRequest>("", getChainalgoStatsRequest, options);
                var chainalgoStats = await getChainalgoStatsResponse.Content.ReadFromJsonAsync<GetChainalgoStats>(options);

                if (blockchainInfo != null)
                {
                    _chainInfoSingleton.currentChainInfo = blockchainInfo.Result;
                    if (_chainInfoSingleton.currentChainInfo != null)
                        _chainInfoSingleton.currentChainInfo.Next_super_block = (uint)Math.Floor(((double)_chainInfoSingleton.currentChainInfo.Blocks / (double)43200) + 1) * 43200;
                }
                else
                    _logger.LogWarning("BlockChainInfo is null");

                if (chainalgoStats != null)
                    _chainInfoSingleton.currentChainAlgoStats = chainalgoStats.Result;
                else
                    _logger.LogWarning("ChainalgoStats is null");

                // TimeSpan not reuired here since we use milliseconds, still put it there to change in future if required
                await Task.Delay(TimeSpan.FromMilliseconds(_explorerConfig.CurrentValue.PullBlockchainInfoDelay));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Can't handle blockchain info");
            }
        }
    }
}