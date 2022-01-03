using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.SignalR;
using ExplorerBackend.Hubs;
using ExplorerBackend.Configs;
using ExplorerBackend.Services.Caching;
using ExplorerBackend.Models.Node;
using ExplorerBackend.Models.Node.Response;

namespace ExplorerBackend.Services.Workers;

public class BlockchainWorker : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IHubContext<EventsHub> _hubContext;
    private readonly IOptionsMonitor<ExplorerConfig> _explorerConfig;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ChaininfoSingleton _chainInfoSingleton;

    public BlockchainWorker(ILogger<BlockchainWorker> logger, IHubContext<EventsHub> hubContext, IOptionsMonitor<ExplorerConfig> explorerConfig, IHttpClientFactory httpClientFactory, ChaininfoSingleton chaininfoSingleton)
    {
        _logger = logger;
        _hubContext = hubContext;
        _explorerConfig = explorerConfig;
        _httpClientFactory = httpClientFactory;
        _chainInfoSingleton = chaininfoSingleton;
    }

    protected override async Task ExecuteAsync(CancellationToken stopToken)
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

                // updating cache

                if (blockchainInfo != null && blockchainInfo.Result != null)
                {
                    _chainInfoSingleton.CurrentChainInfo = blockchainInfo.Result;
                    if (_chainInfoSingleton.CurrentChainInfo != null)
                        _chainInfoSingleton.CurrentChainInfo.Next_super_block = (uint)Math.Floor(((double)_chainInfoSingleton.CurrentChainInfo.Blocks / (double)43200) + 1) * 43200;

                    if (_chainInfoSingleton.LastSyncedBlockOnNode < _chainInfoSingleton.CurrentChainInfo?.Blocks)
                    {
                        _chainInfoSingleton.LastSyncedBlockOnNode = (int)(_chainInfoSingleton.CurrentChainInfo?.Blocks ?? 0);
                        // temporaly commented, moved to blocksworker
                        /*try
                        {
                            await _hubContext.Clients.Group(EventsHub.BackgroundDataChannel).SendAsync("blockchainInfoUpdated", _chainInfoSingleton.CurrentChainInfo);
                        }
                        catch
                        {

                        }*/
                    }
                }
                else
                    _logger.LogWarning("BlockChainInfo is null");

                if (chainalgoStats != null && chainalgoStats.Result != null)
                    _chainInfoSingleton.CurrentChainAlgoStats = chainalgoStats.Result;
                else
                    _logger.LogWarning("ChainalgoStats is null");

                // TimeSpan not reuired here since we use milliseconds, still put it there to change in future if required
                await Task.Delay(TimeSpan.FromMilliseconds(_explorerConfig.CurrentValue.PullBlockchainInfoDelay));
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Can't handle blockchain info");
            }
        }
    }
}