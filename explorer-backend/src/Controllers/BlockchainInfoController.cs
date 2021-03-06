using Microsoft.AspNetCore.Mvc;
using ExplorerBackend.Models.API;
using ExplorerBackend.Services.Caching;

namespace ExplorerBackend.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Produces("application/json")]
public class BlockchainInfoController : ControllerBase
{

    private readonly ILogger _logger;
    private readonly ChaininfoSingleton _chainInfoSingleton;

    public BlockchainInfoController(ILogger<BlockchainInfoController> logger, ChaininfoSingleton chainInfoSingleton)
    {
        _logger = logger;
        _chainInfoSingleton = chainInfoSingleton;
    }

    [HttpGet(Name = "BlockchainInfo")]
    [ProducesResponseType(typeof(BlockchainInfo), StatusCodes.Status200OK)]
    public BlockchainInfo Get()
    {
        return new BlockchainInfo
        {
            CurrentSyncedBlock = _chainInfoSingleton.CurrentSyncedBlock,
            ChainInfo = _chainInfoSingleton.CurrentChainInfo,
            AlgoStats = _chainInfoSingleton.CurrentChainAlgoStats
        };
    }
}
