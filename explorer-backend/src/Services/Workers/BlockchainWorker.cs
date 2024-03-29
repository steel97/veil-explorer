using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.SignalR;
using ExplorerBackend.Hubs;
using ExplorerBackend.Configs;
using ExplorerBackend.Services.Caching;
using ExplorerBackend.Services.Core;

namespace ExplorerBackend.Services.Workers;

public class BlockchainWorker : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IHubContext<EventsHub> _hubContext;
    private readonly IOptionsMonitor<ExplorerConfig> _explorerConfig;
    private readonly ChaininfoSingleton _chainInfoSingleton;
    private readonly NodeRequester _nodeRequester;

    public BlockchainWorker(ILogger<BlockchainWorker> logger, IHubContext<EventsHub> hubContext, IOptionsMonitor<ExplorerConfig> explorerConfig,
        ChaininfoSingleton chaininfoSingleton, NodeRequester nodeRequester)
    {
        _logger = logger;
        _hubContext = hubContext;
        _explorerConfig = explorerConfig;
        _chainInfoSingleton = chaininfoSingleton;
        _nodeRequester = nodeRequester; ;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // get blockchain info
                var blockchainInfo = await _nodeRequester.GetBlockChainInfo(cancellationToken);

                //  chainalgo stats
                var chainalgoStats = await _nodeRequester.GetChainAlgoStats(cancellationToken);


                // updating cache
                if (blockchainInfo != null && blockchainInfo.Result != null)
                {
                    _chainInfoSingleton.CurrentChainInfo = blockchainInfo.Result;
                    _chainInfoSingleton.CurrentChainInfo.Next_super_block = (uint)Math.Floor((_chainInfoSingleton.CurrentChainInfo.Blocks / (double)43200) + 1) * 43200;
                    if (_chainInfoSingleton.LastSyncedBlockOnNode < _chainInfoSingleton.CurrentChainInfo?.Blocks)
                        _chainInfoSingleton.LastSyncedBlockOnNode = (int)(_chainInfoSingleton.CurrentChainInfo?.Blocks ?? 0);

                    var sendUpdate = false;
                    await _chainInfoSingleton.BlockchainDataSemaphore.WaitAsync(cancellationToken);

                    if (_chainInfoSingleton.BlockchainDataShouldBroadcast)
                    {
                        _chainInfoSingleton.BlockchainDataShouldBroadcast = false;
                        sendUpdate = true;
                    }

                    _chainInfoSingleton.BlockchainDataSemaphore.Release();

                    if (sendUpdate)
                    {
                        try
                        {
                            await _hubContext.Clients.Group(EventsHub.BackgroundDataChannel).SendAsync("blockchainInfoUpdated", _chainInfoSingleton.CurrentChainInfo, cancellationToken);
                        }
                        catch
                        {

                        }
                    }
                }
                //else
                //    _logger.LogWarning("BlockChainInfo is null");

                if (chainalgoStats != null && chainalgoStats.Result != null)
                    _chainInfoSingleton.CurrentChainAlgoStats = chainalgoStats.Result;
                //else
                //    _logger.LogWarning("ChainalgoStats is null");

            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Can't handle blockchain info");
            }

            try
            {
                // TimeSpan not reuired here since we use milliseconds, still put it there to change in future if required
                await Task.Delay(TimeSpan.FromMilliseconds(_explorerConfig.CurrentValue.PullBlockchainInfoDelay), cancellationToken);
            }
            catch (OperationCanceledException)
            {

            }
        }

    }
}