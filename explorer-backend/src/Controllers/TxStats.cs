using Microsoft.AspNetCore.Mvc;
using ExplorerBackend.Models.API;
using ExplorerBackend.Services.Caching;

namespace ExplorerBackend.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Produces("application/json")]
public class TxStatsController(ChaininfoSingleton chainInfoSingleton) : ControllerBase
{
    private readonly ChaininfoSingleton _chainInfoSingleton = chainInfoSingleton;

    [HttpGet(Name = "TxStats")]
    [ProducesResponseType(typeof(TxStatsComposite), StatusCodes.Status200OK)]
    public TxStatsComposite Get() => _chainInfoSingleton.CurrentChainStats ?? new TxStatsComposite();

}
