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
        var dict = new Dictionary<string, double>();
        if (_chainInfoSingleton.CurrentChainInfo != null)
        {
            // progpow
            {
                var calc = _chainInfoSingleton.CurrentChainInfo.Difficulty_progpow / VeilStructs.VeilChainParams.nProgPowTargetSpacing;
                calc *= Math.Pow(2, 32);
                dict.Add("progpow", calc);
            }
            // randomx
            {
                var calc = _chainInfoSingleton.CurrentChainInfo.Difficulty_randomx / VeilStructs.VeilChainParams.nRandomXTargetSpacing;
                dict.Add("randomx", calc);
            }
            // sha256d
            {
                var calc = _chainInfoSingleton.CurrentChainInfo.Difficulty_sha256d / VeilStructs.VeilChainParams.nSha256DTargetSpacing;
                dict.Add("sha256d", calc);
            }
        }

        return new BlockchainInfo
        {
            CurrentSyncedBlock = _chainInfoSingleton.CurrentSyncedBlock,
            ChainInfo = _chainInfoSingleton.CurrentChainInfo,
            AlgoStats = _chainInfoSingleton.CurrentChainAlgoStats,
            NetworkHashrates = dict
        };
    }
}
