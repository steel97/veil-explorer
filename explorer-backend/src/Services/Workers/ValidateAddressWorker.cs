using System.Text.Json;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using explorer_backend.Configs;
using explorer_backend.Services.Caching;
using explorer_backend.Services.Queues;

namespace explorer_backend.Services.Workers;

public class ValidateAddressWorker : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IOptionsMonitor<ExplorerConfig> _explorerConfig;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly NodeApiCacheSingleton _nodeApiCacheSingleton;
    private readonly IBackgroundTaskQueue _taskQueue;

    public ValidateAddressWorker(ILogger<ValidateAddressWorker> logger, IOptionsMonitor<ExplorerConfig> explorerConfig, IHttpClientFactory httpClientFactory, NodeApiCacheSingleton nodeApiCacheSingleton, ValidateAddressBackgroundTaskQueue taskQueue)
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
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Can't handle addressvalidate queue");
            }
        }
    }
}