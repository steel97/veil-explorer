using System.Text.Json;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using ExplorerBackend.Configs;
using ExplorerBackend.Services.Caching;
using ExplorerBackend.Models.Node;
using ExplorerBackend.Models.Node.Response;

namespace ExplorerBackend.Services.Workers;

public class MempoolWorker : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IOptionsMonitor<ExplorerConfig> _explorerConfig;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ChaininfoSingleton _chainInfoSingleton;

    public MempoolWorker(ILogger<MempoolWorker> logger, IOptionsMonitor<ExplorerConfig> explorerConfig, IHttpClientFactory httpClientFactory, ChaininfoSingleton chaininfoSingleton)
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
                var getRawMempoolRequest = new JsonRPCRequest
                {
                    Id = 1,
                    Method = "getrawmempool",
                    Params = new List<object>(new object[] { false })
                };
                var getRawMempoolResult = await httpClient.PostAsJsonAsync<JsonRPCRequest>("", getRawMempoolRequest, options);
                var mempoolInfo = await getRawMempoolResult.Content.ReadFromJsonAsync<GetRawMempool>(options);
                // get chainalgo stats

                if (mempoolInfo != null && mempoolInfo.Result != null)
                {
                    var unconfirmedTxs = new List<GetRawTransactionResult>();
                    foreach (var txId in mempoolInfo.Result)
                    {
                        var getRawTransactionRequest = new JsonRPCRequest
                        {
                            Id = 1,
                            Method = "getrawtransaction",
                            Params = new List<object>(new object[] { txId, true })
                        };
                        var getRawTransactionResponse = await httpClient.PostAsJsonAsync<JsonRPCRequest>("", getRawTransactionRequest, options);
                        var rawTransaction = await getRawTransactionResponse.Content.ReadFromJsonAsync<GetRawTransaction>(options);
                        if (rawTransaction != null && rawTransaction.Result != null)
                            unconfirmedTxs.Add(rawTransaction.Result);
                    }


                    _chainInfoSingleton.UnconfirmedTxs = unconfirmedTxs;

                }

                // TimeSpan not reuired here since we use milliseconds, still put it there to change in future if required
                await Task.Delay(TimeSpan.FromMilliseconds(_explorerConfig.CurrentValue.PullMempoolDelay));
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Can't handle mempool info");
            }
        }
    }
}