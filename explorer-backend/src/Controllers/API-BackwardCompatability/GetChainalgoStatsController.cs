using Microsoft.AspNetCore.Mvc;
using explorer_backend.Models.Node.Response;
using explorer_backend.Services.Caching;

namespace explorer_backend.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Produces("application/json")]
public class GetChainalgoStatsController : ControllerBase
{

    private readonly ILogger _logger;
    private readonly ChaininfoSingleton _chainInfoSingleton;

    public GetChainalgoStatsController(ILogger<GetChainalgoStatsController> logger, ChaininfoSingleton chainInfoSingleton)
    {
        _logger = logger;
        _chainInfoSingleton = chainInfoSingleton;
    }

    [HttpGet(Name = "GetChainalgoStats")]
    [ProducesResponseType(typeof(GetChainalgoStatsResult), StatusCodes.Status200OK)]
    public GetChainalgoStatsResult? Get()
    {
        return _chainInfoSingleton.currentChainAlgoStats;
    }
}
