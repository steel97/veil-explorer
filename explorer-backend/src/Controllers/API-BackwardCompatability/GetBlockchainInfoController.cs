using Microsoft.AspNetCore.Mvc;
using explorer_backend.Models.Node.Response;
using explorer_backend.Services.Caching;

namespace explorer_backend.Controllers;

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
    public GetBlockchainInfoResult? Get()
    {
        return _chainInfoSingleton.currentChainInfo;
    }
}
