using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ExplorerBackend.Services.Caching;
using ExplorerBackend.Configs;

namespace ExplorerBackend.Controllers;

// TO-DO replace internalkey validation with middleware
[ApiController]
[Route("/api/internal/[controller]")]
[Produces("application/json")]
public class BackendState : ControllerBase
{

    private readonly ILogger _logger;
    private readonly IOptions<ServerConfig> _serverConfig;
    private readonly InternalSingleton _internalSingleton;

    public BackendState(ILogger<TxController> logger, IOptions<ServerConfig> serverConfig, InternalSingleton internalSingleton)
    {
        _logger = logger;
        _serverConfig = serverConfig;
        _internalSingleton = internalSingleton;
    }

    [HttpGet(Name = "BackendState")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get(string accessKey, CancellationToken cancellationToken)
    {
        if (accessKey != _serverConfig.Value.InternalAccessKey) return Problem("invalid access key", statusCode: 400);

        return Ok(_internalSingleton);
    }
}
