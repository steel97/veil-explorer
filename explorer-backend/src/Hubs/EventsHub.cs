using Microsoft.AspNetCore.SignalR;
using ExplorerBackend.Services.Caching;

namespace ExplorerBackend.Hubs;

public class EventsHub : Hub
{
    public const string BackgroundDataChannel = "background_data";
    public const string BlocksDataChannel = "blocks_update";

    private readonly ChaininfoSingleton _chainInfoSingleton;
    private readonly ILogger _logger;

    public EventsHub(ILogger<EventsHub> logger, ChaininfoSingleton chainInfoSingleton)
    {
        _chainInfoSingleton = chainInfoSingleton;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, BackgroundDataChannel);
        await Groups.AddToGroupAsync(Context.ConnectionId, BlocksDataChannel);
    }
}