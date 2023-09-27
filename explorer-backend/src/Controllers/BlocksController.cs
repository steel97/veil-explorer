using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ExplorerBackend.Models.API;
using ExplorerBackend.Configs;
using ExplorerBackend.Models.Data;
using ExplorerBackend.Services.Data;

namespace ExplorerBackend.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Produces("application/json")]
public class BlocksController : ControllerBase
{
    private readonly IOptions<APIConfig> _apiConfig;
    private readonly IBlocksDataService _blocksDataService; // switched to the new layer

    public BlocksController(IOptions<APIConfig> apiConfig, IBlocksDataService blocksDataService)
    {
        _apiConfig = apiConfig;
        _blocksDataService = blocksDataService;
    }

    [HttpGet(Name = "GetBlocks")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(List<SimplifiedBlock>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(int offset, int count, SortDirection sort, CancellationToken cancellationToken)
    {
        if (offset < 0)
            return Problem("offset should be higher or equal to zero", statusCode: 400);
        if (count > _apiConfig.Value.MaxTransactionsPullCount || count < 1)
            return Problem($"count should be between 1 and {_apiConfig.Value.MaxBlocksPullCount} ", statusCode: 400);
        
        return Ok(await _blocksDataService.GetSimplifiedBlocksAsync(offset, count, sort, cancellationToken));
    }
}
