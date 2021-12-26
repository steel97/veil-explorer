using System.Text.Json;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using ExplorerBackend.Configs;
using ExplorerBackend.Services.Caching;
using ExplorerBackend.Services.Queues;

namespace ExplorerBackend.Services.Workers;

public class ScanTxOutsetWorker : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IOptionsMonitor<ExplorerConfig> _explorerConfig;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly NodeApiCacheSingleton _nodeApiCacheSingleton;
    private readonly IBackgroundTaskQueue _taskQueue;

    public ScanTxOutsetWorker(ILogger<ScanTxOutsetWorker> logger, IOptionsMonitor<ExplorerConfig> explorerConfig, IHttpClientFactory httpClientFactory, NodeApiCacheSingleton nodeApiCacheSingleton, ScanTxOutsetBackgroundTaskQueue taskQueue)
    {
        _logger = logger;
        _explorerConfig = explorerConfig;
        _httpClientFactory = httpClientFactory;
        _nodeApiCacheSingleton = nodeApiCacheSingleton;
        _taskQueue = taskQueue;
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
                Func<CancellationToken, ValueTask>? workItem = await _taskQueue.DequeueAsync(stopToken);
                await workItem(stopToken);

                await Task.Delay(TimeSpan.FromMilliseconds(_explorerConfig.CurrentValue.NodeWorkersPullDelay));
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Can't handle txoutset queue");
            }
        }
    }
}