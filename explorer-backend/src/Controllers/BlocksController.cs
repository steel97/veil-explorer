using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ExplorerBackend.Models.API;
using ExplorerBackend.Configs;
using ExplorerBackend.Models.Data;
using ExplorerBackend.Persistence.Repositories;

namespace ExplorerBackend.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Produces("application/json")]
public class BlocksController : ControllerBase
{
    private readonly IOptions<APIConfig> _apiConfig;
    private readonly IBlocksRepository _blocksRepository;

    public BlocksController(IOptions<APIConfig> apiConfig, IBlocksRepository blocksRepository)
    {
        _apiConfig = apiConfig;
        _blocksRepository = blocksRepository;
    }

    [HttpGet(Name = "GetBlocks")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(List<SimplifiedBlock>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(int offset, int count, SortDirection sort, CancellationToken cancellationToken)
    {
        if (offset < 0)
            return Problem("offset should be higher or equal to zero", statusCode: 400);
        if (count < 1)
            return Problem("count should be more or equal to one", statusCode: 400);
        if (count > _apiConfig.Value.MaxBlocksPullCount)
            return Problem($"count should be less or equal than {_apiConfig.Value.MaxBlocksPullCount}", statusCode: 400);

        return Ok(await _blocksRepository.GetSimplifiedBlocksAsync(offset, count, sort, cancellationToken));
    }
}
