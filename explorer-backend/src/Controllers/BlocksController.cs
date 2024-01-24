using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ExplorerBackend.Models.API;
using ExplorerBackend.Configs;
using ExplorerBackend.Models.Data;
using ExplorerBackend.Services.Data;
using ExplorerBackend.Services.Caching;

namespace ExplorerBackend.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Produces("application/json")]
public class BlocksController : ControllerBase
{
    private readonly IOptions<APIConfig> _apiConfig;
    private readonly ChaininfoSingleton _chaininfoSingleton;
    private readonly IBlocksDataService _blocksDataService;

    public BlocksController(IOptions<APIConfig> apiConfig, ChaininfoSingleton chaininfoSingleton, IBlocksDataService blocksDataService)
    {
        _apiConfig = apiConfig;
        _chaininfoSingleton = chaininfoSingleton;
        _blocksDataService = blocksDataService;
    }

    [HttpGet(Name = "GetBlocks")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(List<SimplifiedBlock>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(int offset, int count, SortDirection sort, CancellationToken cancellationToken)
    {
        int calculatedHeight = sort == SortDirection.DESC ? _chaininfoSingleton.CurrentSyncedBlock - offset - count : 1 + offset + count;

        if (calculatedHeight < 0 || calculatedHeight > _chaininfoSingleton.CurrentSyncedBlock)
            return Problem($"offset should be higher or equal to 0 and less than {_chaininfoSingleton.CurrentSyncedBlock}", statusCode: 400);
        if (count > _apiConfig.Value.MaxTransactionsPullCount || count < 1)
            return Problem($"count should be between 1 and {_apiConfig.Value.MaxBlocksPullCount} ", statusCode: 400);
        
        return Ok(await _blocksDataService.GetSimplifiedBlocksAsync(offset, count, sort, cancellationToken));
    }
}
