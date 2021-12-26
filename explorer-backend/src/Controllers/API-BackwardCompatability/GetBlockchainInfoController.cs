using Microsoft.AspNetCore.Mvc;
using ExplorerBackend.Models.Node.Response;
using ExplorerBackend.Services.Caching;

namespace ExplorerBackend.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Produces("application/json")]
public class GetBlockchainInfoController : ControllerBase
{

    private readonly ILogger _logger;
    private readonly ChaininfoSingleton _chainInfoSingleton;

    public GetBlockchainInfoController(ILogger<GetBlockchainInfoController> logger, ChaininfoSingleton chainInfoSingleton)
    {
        _logger = logger;
        _chainInfoSingleton = chainInfoSingleton;
    }

    [HttpGet(Name = "GetBlockchainInfo")]
    [ProducesResponseType(typeof(GetBlockchainInfoResult), StatusCodes.Status200OK)]
    public GetBlockchainInfoResult? Get() => _chainInfoSingleton.currentChainInfo;

}
