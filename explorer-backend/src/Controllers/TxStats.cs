using Microsoft.AspNetCore.Mvc;
using ExplorerBackend.Models.API;
using ExplorerBackend.Services.Caching;

namespace ExplorerBackend.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Produces("application/json")]
public class TxStatsController : ControllerBase
{
    private readonly ChaininfoSingleton _chainInfoSingleton;

    public TxStatsController(ChaininfoSingleton chainInfoSingleton)
    {
        _chainInfoSingleton = chainInfoSingleton;
    }

    [HttpGet(Name = "TxStats")]
    [ProducesResponseType(typeof(TxStatsComposite), StatusCodes.Status200OK)]
    public TxStatsComposite Get() => _chainInfoSingleton.CurrentChainStats ?? new TxStatsComposite();

}
