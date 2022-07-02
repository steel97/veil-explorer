using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ExplorerBackend.Models.System;
using ExplorerBackend.Configs;

namespace ExplorerBackend.Controllers;

// TO-DO replace internalkey validation with middleware
[ApiController]
[Route("/api/internal/[controller]")]
[Produces("application/json")]
public class FetchExportedTxs : ControllerBase
{

    private readonly ILogger _logger;
    private readonly IOptions<ServerConfig> _serverConfig;

    public FetchExportedTxs(ILogger<TxController> logger, IOptions<ServerConfig> serverConfig)
    {
        _logger = logger;
        _serverConfig = serverConfig;
    }

    [HttpGet(Name = "FetchExportedTxs")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get(string accessKey, int internalId, CancellationToken cancellationToken)
    {
        if (accessKey != _serverConfig.Value.InternalAccessKey) return Problem("invalid access key", statusCode: 400);
        if (!System.IO.File.Exists("./data/export-txs-" + internalId + ".xlsx")) return Problem("can't find file", statusCode: 400);

        return File(System.IO.File.ReadAllBytes("./data/export-txs-" + internalId + ".xlsx"), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }
}
