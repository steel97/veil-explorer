using Microsoft.Extensions.Options;
using ExplorerBackend.Configs;
using ExplorerBackend.Models.System;
using ExplorerBackend.Services.Core;
using ExplorerBackend.Services.Queues;
using ExplorerBackend.Services.Caching;

namespace ExplorerBackend.Services.Workers;

public class ScanTxOutsetWorker(ILogger<ScanTxOutsetWorker> logger, IOptionsMonitor<ExplorerConfig> explorerConfig, ScanTxOutsetBackgroundTaskQueue taskQueue, NodeApiCacheSingleton nodeApiCacheSingleton, NodeRequester nodeRequester) : BackgroundService
{
    private readonly ILogger _logger = logger;
    private readonly IOptionsMonitor<ExplorerConfig> _explorerConfig = explorerConfig;
    private readonly IBackgroundTaskQueue _taskQueue = taskQueue;
    private readonly NodeApiCacheSingleton _nodeApiCacheSingleton = nodeApiCacheSingleton;
    private readonly NodeRequester _nodeRequester = nodeRequester;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var scanTxOutsetBridge = new ScanTxOutsetBridge
        {
            NodeApiCacheLink = _nodeApiCacheSingleton,
            NodeRequesterLink = _nodeRequester
        };

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                Func<object, CancellationToken, ValueTask>? workItem = await _taskQueue.DequeueAsync(cancellationToken);
                await workItem(scanTxOutsetBridge, cancellationToken);

                await Task.Delay(TimeSpan.FromMilliseconds(_explorerConfig.CurrentValue.NodeWorkersPullDelay), cancellationToken);
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