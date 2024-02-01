using Microsoft.Extensions.Options;
using ExplorerBackend.Core;
using ExplorerBackend.Configs;
using ExplorerBackend.Models.System;
using ExplorerBackend.Services.Caching;
using ExplorerBackend.Services.Queues;
using ExplorerBackend.Models.Node.Response;

namespace ExplorerBackend.Services.Workers;

public class SupplyWorker(ILogger<SupplyWorker> logger, IOptionsMonitor<ExplorerConfig> explorerConfig, IOptionsMonitor<APIConfig> apiConfig, ChaininfoSingleton chaininfoSingleton,
    ScanTxOutsetBackgroundTaskQueue scanTxOutsetBackgroundTaskQueue, NodeApiCacheSingleton nodeApiCacheSingleton) : BackgroundService
{
    private readonly ILogger _logger = logger;
    private readonly IOptionsMonitor<ExplorerConfig> _explorerConfig = explorerConfig;
    private readonly IOptionsMonitor<APIConfig> _apiConfig = apiConfig;
    private readonly ChaininfoSingleton _chainInfoSingleton = chaininfoSingleton;
    private readonly ScanTxOutsetBackgroundTaskQueue _scanTxOutsetBackgroundTaskQueue = scanTxOutsetBackgroundTaskQueue;
    private readonly NodeApiCacheSingleton _nodeApiCacheSingleton = nodeApiCacheSingleton;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.BudgetAddress);
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.FoundationAddress);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                ScanTxOutset? scanTxOutsetBudgetRes = null;
                if (!_nodeApiCacheSingleton.IsInQueue($"scantxoutset-{_explorerConfig.CurrentValue.BudgetAddress}"))
                {
                    try
                    {

                        // try get balance
                        var scanTxOutsetFlag = new AsyncFlag
                        {
                            State = false
                        };
                        await _scanTxOutsetBackgroundTaskQueue.QueueBackgroundWorkItemAsync(async (input, token) =>
                        {
                            var bridge = (ScanTxOutsetBridge)input;
                            if (bridge == null || bridge.NodeApiCacheLink == null || bridge.NodeRequesterLink == null) return;

                            if (await bridge.NodeApiCacheLink.PutInQueueAsync($"scantxoutset-{_explorerConfig.CurrentValue.BudgetAddress}"))
                            {
                                await bridge.NodeRequesterLink.ScanTxOutsetAndCacheAsync(_explorerConfig.CurrentValue.BudgetAddress, token);
                                await bridge.NodeApiCacheLink.RemoveFromQueueAsync($"scantxoutset-{_explorerConfig.CurrentValue.BudgetAddress}");
                            }

                            if (scanTxOutsetFlag != null)
                                scanTxOutsetFlag.State = true;
                        });
                        await AsyncUtils.WaitUntilAsync(cancellationToken, () => scanTxOutsetFlag.State, _apiConfig.CurrentValue.ApiQueueSpinDelay, _apiConfig.CurrentValue.ApiQueueSystemWaitTimeout);
                    }
                    catch (TimeoutException)
                    {

                    }
                }

                scanTxOutsetBudgetRes = _nodeApiCacheSingleton.GetApiCache<ScanTxOutset>($"scantxoutset-{_explorerConfig.CurrentValue.BudgetAddress}");



                ScanTxOutset? scanTxOutsetFoundationRes = null;
                if (!_nodeApiCacheSingleton.IsInQueue($"scantxoutset-{_explorerConfig.CurrentValue.FoundationAddress}"))
                {
                    try
                    {
                        // try get balance
                        var scanTxOutsetFlag = new AsyncFlag
                        {
                            State = false
                        };
                        await _scanTxOutsetBackgroundTaskQueue.QueueBackgroundWorkItemAsync(async (input, token) =>
                        {
                            var bridge = (ScanTxOutsetBridge)input;
                            if (bridge == null || bridge.NodeApiCacheLink == null || bridge.NodeRequesterLink == null) return;

                            if (await bridge.NodeApiCacheLink.PutInQueueAsync($"scantxoutset-{_explorerConfig.CurrentValue.FoundationAddress}"))
                            {
                                await bridge.NodeRequesterLink.ScanTxOutsetAndCacheAsync(_explorerConfig.CurrentValue.FoundationAddress, token);
                                await bridge.NodeApiCacheLink.RemoveFromQueueAsync($"scantxoutset-{_explorerConfig.CurrentValue.FoundationAddress}");
                            }

                            if (scanTxOutsetFlag != null)
                                scanTxOutsetFlag.State = true;
                        });
                        await AsyncUtils.WaitUntilAsync(cancellationToken, () => scanTxOutsetFlag.State, _apiConfig.CurrentValue.ApiQueueSpinDelay, _apiConfig.CurrentValue.ApiQueueSystemWaitTimeout);
                    }
                    catch (TimeoutException)
                    {

                    }
                }

                scanTxOutsetFoundationRes = _nodeApiCacheSingleton.GetApiCache<ScanTxOutset>($"scantxoutset-{_explorerConfig.CurrentValue.FoundationAddress}");


                if (scanTxOutsetBudgetRes != null && scanTxOutsetBudgetRes.Result != null)
                    _chainInfoSingleton.BudgetWalletAmount = scanTxOutsetBudgetRes.Result.total_amount;
                if (scanTxOutsetFoundationRes != null && scanTxOutsetFoundationRes.Result != null)
                    _chainInfoSingleton.FoundationWalletAmmount = scanTxOutsetFoundationRes.Result.total_amount;

                await Task.Delay(TimeSpan.FromMilliseconds(_explorerConfig.CurrentValue.SupplyPullDelay), cancellationToken);
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Can't handle supply");
            }
        }
    }
}