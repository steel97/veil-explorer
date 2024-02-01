using Microsoft.AspNetCore.Mvc;
using ExplorerBackend.Models.Node.Response;
using ExplorerBackend.Services.Caching;

namespace ExplorerBackend.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Produces("application/json")]
public class GetChainalgoStatsController(ChaininfoSingleton chainInfoSingleton) : ControllerBase
{
    private readonly ChaininfoSingleton _chainInfoSingleton = chainInfoSingleton;

    [HttpGet(Name = "GetChainalgoStats")]
    [ProducesResponseType(typeof(GetChainalgoStatsResult), StatusCodes.Status200OK)]
    public GetChainalgoStatsResult? Get() => _chainInfoSingleton.CurrentChainAlgoStats;
}
