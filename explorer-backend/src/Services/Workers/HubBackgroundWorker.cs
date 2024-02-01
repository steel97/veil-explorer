using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.SignalR;
using ExplorerBackend.Hubs;
using ExplorerBackend.Configs;
using ExplorerBackend.Services.Caching;

namespace ExplorerBackend.Services.Workers;

public class HubBackgroundWorker(ILogger<HubBackgroundWorker> logger, IHubContext<EventsHub> hubContext, IOptionsMonitor<ExplorerConfig> explorerConfig, ChaininfoSingleton chaininfoSingleton) : BackgroundService
{
    private readonly ILogger _logger = logger;
    private readonly IHubContext<EventsHub> _hubContext = hubContext;
    private readonly IOptionsMonitor<ExplorerConfig> _explorerConfig = explorerConfig;
    private readonly ChaininfoSingleton _chainInfoSingleton = chaininfoSingleton;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                try
                {
                    await _hubContext.Clients.Group(EventsHub.BackgroundDataChannel).SendAsync("backgroundInfoUpdated", _chainInfoSingleton.CurrentSyncedBlock, _chainInfoSingleton.CurrentChainAlgoStats, cancellationToken);
                }
                catch
                {

                }

                // TimeSpan not reuired here since we use milliseconds, still put it there to change in future if required
                await Task.Delay(TimeSpan.FromMilliseconds(_explorerConfig.CurrentValue.HubNotifyDelay), cancellationToken);
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hub background worker failed");
            }
        }
    }
}