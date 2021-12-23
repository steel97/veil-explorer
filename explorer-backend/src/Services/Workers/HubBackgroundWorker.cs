using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.SignalR;
using explorer_backend.Hubs;
using explorer_backend.Configs;
using explorer_backend.Services.Caching;

namespace explorer_backend.Services.Workers;

public class HubBackgroundWorker : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IHubContext<EventsHub> _hubContext;
    private readonly IOptionsMonitor<ExplorerConfig> _explorerConfig;
    private readonly ChaininfoSingleton _chainInfoSingleton;

    public HubBackgroundWorker(ILogger<HubBackgroundWorker> logger, IHubContext<EventsHub> hubContext, IOptionsMonitor<ExplorerConfig> explorerConfig, ChaininfoSingleton chaininfoSingleton)
    {
        _logger = logger;
        _hubContext = hubContext;
        _explorerConfig = explorerConfig;
        _chainInfoSingleton = chaininfoSingleton;
    }
    protected override async Task ExecuteAsync(CancellationToken stopToken)
    {
        while (!stopToken.IsCancellationRequested)
        {
            try
            {
                try
                {
                    await _hubContext.Clients.Group("chaininfo").SendAsync("BlockchainInfoUpdated", _chainInfoSingleton.currentSyncedBlock, _chainInfoSingleton.currentChainInfo, _chainInfoSingleton.currentChainAlgoStats);
                }
                catch
                {

                }

                // TimeSpan not reuired here since we use milliseconds, still put it there to change in future if required
                await Task.Delay(TimeSpan.FromMilliseconds(_explorerConfig.CurrentValue.HubNotifyDelay));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hub background worker failed");
            }
        }
    }
}