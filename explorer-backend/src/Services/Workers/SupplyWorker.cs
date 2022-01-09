using Microsoft.Extensions.Options;
using ExplorerBackend.Core;
using ExplorerBackend.Configs;
using ExplorerBackend.Services.Caching;
using ExplorerBackend.Services.Queues;
using ExplorerBackend.Services.Core;
using ExplorerBackend.Models.Node.Response;

namespace ExplorerBackend.Services.Workers;

public class SupplyWorker : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IOptionsMonitor<ExplorerConfig> _explorerConfig;
    private readonly IOptionsMonitor<APIConfig> _apiConfig;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ChaininfoSingleton _chainInfoSingleton;
    private readonly ScanTxOutsetBackgroundTaskQueue _scanTxOutsetBackgroundTaskQueue;
    private readonly INodeRequester _nodeRequester;
    private readonly NodeApiCacheSingleton _nodeApiCacheSingleton;

    public SupplyWorker(ILogger<SupplyWorker> logger, IOptionsMonitor<ExplorerConfig> explorerConfig, IOptionsMonitor<APIConfig> apiConfig, IHttpClientFactory httpClientFactory, ChaininfoSingleton chaininfoSingleton,
        ScanTxOutsetBackgroundTaskQueue scanTxOutsetBackgroundTaskQueue,
        INodeRequester nodeRequester, NodeApiCacheSingleton nodeApiCacheSingleton)
    {
        _logger = logger;
        _explorerConfig = explorerConfig;
        _apiConfig = apiConfig;
        _httpClientFactory = httpClientFactory;
        _chainInfoSingleton = chaininfoSingleton;
        _scanTxOutsetBackgroundTaskQueue = scanTxOutsetBackgroundTaskQueue;
        _nodeRequester = nodeRequester;
        _nodeApiCacheSingleton = nodeApiCacheSingleton;
    }

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
                        if (await _nodeApiCacheSingleton.PutInQueueAsync($"scantxoutset-{_explorerConfig.CurrentValue.BudgetAddress}"))
                        {
                            // try get balance
                            var scanTxOutsetFlag = new AsyncFlag
                            {
                                State = false
                            };
                            await _scanTxOutsetBackgroundTaskQueue.QueueBackgroundWorkItemAsync(async token =>
                            {
                                try
                                {
                                    await _nodeRequester.ScanTxOutsetAndCacheAsync(_explorerConfig.CurrentValue.BudgetAddress, token);
                                }
                                catch
                                {

                                }

                                await _nodeApiCacheSingleton.RemoveFromQueueAsync($"scantxoutset-{_explorerConfig.CurrentValue.BudgetAddress}");

                                if (scanTxOutsetFlag != null)
                                    scanTxOutsetFlag.State = true;
                            });
                            await AsyncUtils.WaitUntilAsync(cancellationToken, () => scanTxOutsetFlag.State, _apiConfig.CurrentValue.ApiQueueSpinDelay, _apiConfig.CurrentValue.ApiQueueSystemWaitTimeout);
                            scanTxOutsetBudgetRes = _nodeApiCacheSingleton.GetApiCache<ScanTxOutset>($"scantxoutset-{_explorerConfig.CurrentValue.BudgetAddress}");
                        }
                    }
                    catch (TimeoutException)
                    {

                    }
                }



                ScanTxOutset? scanTxOutsetFoundationRes = null;


                if (!_nodeApiCacheSingleton.IsInQueue($"scantxoutset-{_explorerConfig.CurrentValue.FoundationAddress}"))
                {
                    try
                    {
                        if (await _nodeApiCacheSingleton.PutInQueueAsync($"scantxoutset-{_explorerConfig.CurrentValue.FoundationAddress}"))
                        {
                            // try get balance
                            var scanTxOutsetFlag = new AsyncFlag
                            {
                                State = false
                            };
                            await _scanTxOutsetBackgroundTaskQueue.QueueBackgroundWorkItemAsync(async token =>
                            {
                                try
                                {
                                    await _nodeRequester.ScanTxOutsetAndCacheAsync(_explorerConfig.CurrentValue.FoundationAddress, token);
                                }
                                catch
                                {

                                }

                                await _nodeApiCacheSingleton.RemoveFromQueueAsync($"scantxoutset-{_explorerConfig.CurrentValue.FoundationAddress}");

                                if (scanTxOutsetFlag != null)
                                    scanTxOutsetFlag.State = true;
                            });
                            await AsyncUtils.WaitUntilAsync(cancellationToken, () => scanTxOutsetFlag.State, _apiConfig.CurrentValue.ApiQueueSpinDelay, _apiConfig.CurrentValue.ApiQueueSystemWaitTimeout);
                            scanTxOutsetFoundationRes = _nodeApiCacheSingleton.GetApiCache<ScanTxOutset>($"scantxoutset-{_explorerConfig.CurrentValue.FoundationAddress}");
                        }
                    }
                    catch (TimeoutException)
                    {

                    }
                }


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