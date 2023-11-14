using Microsoft.Extensions.Options;
using ExplorerBackend.Configs;
using ExplorerBackend.VeilStructs;
using ExplorerBackend.Services.Caching;
using ExplorerBackend.Models.API;
using ExplorerBackend.Services.Core;

namespace ExplorerBackend.Services.Workers;

public class BlockchainStatsWorker : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IOptionsMonitor<ExplorerConfig> _explorerConfig;
    private readonly NodeRequester _nodeRequester;
    private readonly ChaininfoSingleton _chainInfoSingleton;

    public BlockchainStatsWorker(ILogger<BlockchainStatsWorker> logger, IOptionsMonitor<ExplorerConfig> explorerConfig,
        NodeRequester nodeRequester, ChaininfoSingleton chaininfoSingleton)
    {
        _logger = logger;
        _explorerConfig = explorerConfig;
        _nodeRequester = nodeRequester;
        _chainInfoSingleton = chaininfoSingleton;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var targetBlocksPerDay = 24 * 60 * 60 / Constants.BLOCK_TIME;

        // let other services warm up, this allows us not to wait PullBlockchainStatsDelay if some data is unavailable at beginning
        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken); 

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var txStatsDay = _nodeRequester.GetTxStatsAsync(targetBlocksPerDay / 4, -144, cancellationToken);
                var txStatsWeek = _nodeRequester.GetTxStatsAsync( targetBlocksPerDay / 4, -144 * 7, cancellationToken);
                var txStatsMonth = _nodeRequester.GetTxStatsAsync(targetBlocksPerDay / 4, -144 * 30, cancellationToken);
                var txStatsOverall = _nodeRequester.GetTxStatsAsync(_explorerConfig.CurrentValue.StatsPointsCount, 0, cancellationToken);

                await Task.WhenAll(txStatsDay, txStatsMonth, txStatsWeek, txStatsOverall);

                var finalDict = new Dictionary<string, TxStatsEntry>()
                {
                    {"day", txStatsDay.Result},
                    {"week", txStatsWeek.Result},
                    {"month", txStatsMonth.Result},
                    {"overall", txStatsOverall.Result}
                };

                _chainInfoSingleton.CurrentChainStats = new TxStatsComposite
                {
                    TxStats = finalDict
                };

                // TimeSpan not reuired here since we use milliseconds, still put it there to change in future if required
                await Task.Delay(TimeSpan.FromMilliseconds(_explorerConfig.CurrentValue.PullBlockchainStatsDelay), cancellationToken);
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Can't handle blockchain stats info");
            }
        }
    }
}