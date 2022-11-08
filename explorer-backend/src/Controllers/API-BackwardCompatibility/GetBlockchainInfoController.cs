using Microsoft.AspNetCore.Mvc;
using ExplorerBackend.Models.Node.Response;
using ExplorerBackend.Services.Caching;

namespace ExplorerBackend.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Produces("application/json")]
public class GetBlockchainInfoController : ControllerBase
{
    private readonly ChaininfoSingleton _chainInfoSingleton;

    public GetBlockchainInfoController(ChaininfoSingleton chainInfoSingleton)
    {
        _chainInfoSingleton = chainInfoSingleton;
    }

    [HttpGet(Name = "GetBlockchainInfo")]
    [ProducesResponseType(typeof(GetBlockchainInfoResult), StatusCodes.Status200OK)]
    public GetBlockchainInfoResult? Get() => _chainInfoSingleton.CurrentChainInfo;

}
