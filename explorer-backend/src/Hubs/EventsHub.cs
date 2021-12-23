using Microsoft.AspNetCore.SignalR;
using explorer_backend.Services.Caching;

namespace explorer_backend.Hubs;

public class EventsHub : Hub
{
    private readonly ChaininfoSingleton _chainInfoSingleton;
    private readonly ILogger _logger;

    public EventsHub(ILogger<EventsHub> logger, ChaininfoSingleton chainInfoSingleton)
    {
        _chainInfoSingleton = chainInfoSingleton;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "chaininfo");
        await Groups.AddToGroupAsync(Context.ConnectionId, "blocksupdate");
    }
}