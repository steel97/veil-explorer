using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using ExplorerBackend.Configs;
using ExplorerBackend.Services.Caching;
using ExplorerBackend.Models.Node;
using ExplorerBackend.Models.Node.Response;
using ExplorerBackend.Services.Core;

namespace ExplorerBackend.Services.Workers;

public class MempoolWorker : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IOptionsMonitor<ExplorerConfig> _explorerConfig;
    private readonly ChaininfoSingleton _chainInfoSingleton;
    private readonly NodeRequester _nodeRequester;

    public MempoolWorker(ILogger<MempoolWorker> logger, IOptionsMonitor<ExplorerConfig> explorerConfig, IHttpClientFactory httpClientFactory,
        ChaininfoSingleton chaininfoSingleton, NodeRequester nodeRequester)
    {
        _logger = logger;
        _explorerConfig = explorerConfig;
        _chainInfoSingleton = chaininfoSingleton;
        _nodeRequester = nodeRequester;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // get raw mempool
                var mempoolInfo = await _nodeRequester.GetRawMempool(cancellationToken);

                // get raw transactions
                if (mempoolInfo != null && mempoolInfo.Result != null)
                {
                    var unconfirmedTxs = new List<GetRawTransactionResult>();
                    foreach (var txId in mempoolInfo.Result)
                    {
                        var rawTransaction = await _nodeRequester.GetRawTransaction(txId, cancellationToken);

                        if (rawTransaction != null && rawTransaction.Result != null)
                            unconfirmedTxs.Add(rawTransaction.Result);
                    }

                    _chainInfoSingleton.UnconfirmedTxs = unconfirmedTxs;
                }

                // TimeSpan not reuired here since we use milliseconds, still put it there to change in future if required
                await Task.Delay(TimeSpan.FromMilliseconds(_explorerConfig.CurrentValue.PullMempoolDelay), cancellationToken);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Can't handle mempool info");
            }
        }
    }
}