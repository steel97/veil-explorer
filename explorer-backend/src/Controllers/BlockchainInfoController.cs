using Microsoft.AspNetCore.Mvc;
using explorer_backend.Models.API;
using explorer_backend.Services.Caching;

namespace explorer_backend.Controllers;

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
            CurrentSyncedBlock = _chainInfoSingleton.currentSyncedBlock,
            ChainInfo = _chainInfoSingleton.currentChainInfo,
            AlgoStats = _chainInfoSingleton.currentChainAlgoStats
        };
    }
}
