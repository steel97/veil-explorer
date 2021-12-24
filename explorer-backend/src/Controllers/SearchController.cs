using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using explorer_backend.Models.API;
using explorer_backend.Configs;
using explorer_backend.Services.Core;
using explorer_backend.Persistence.Repositories;

namespace explorer_backend.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Produces("application/json")]
public class SearchController : ControllerBase
{
    private static Regex blockHeightRegex = new("^(0|[1-9][0-9]*)$", RegexOptions.Compiled);
    private readonly ILogger<SearchController> _logger;
    private readonly IOptions<APIConfig> _apiConfig;
    private readonly IBlocksRepository _blocksRepository;
    private readonly ITransactionsRepository _transactionsRepository;
    private readonly IUtilityService _utilityService;

    public SearchController(ILogger<SearchController> logger, IOptions<APIConfig> apiConfig, IBlocksRepository blocksRepository, ITransactionsRepository transactionsRepository, IUtilityService utilityService)
    {
        _logger = logger;
        _apiConfig = apiConfig;
        _blocksRepository = blocksRepository;
        _transactionsRepository = transactionsRepository;
        _utilityService = utilityService;
    }

    [HttpPost(Name = "Search")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(List<SearchResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(SearchRequest body)
    {
        var response = new SearchResponse();
        response.Found = false;
        response.Type = EntityType.UNKNOWN;
        response.Query = body.Query;

        if (body.Query != null)
        {
            if (blockHeightRegex.IsMatch(body.Query))
            {
                response.Found = true;
                response.Type = EntityType.BLOCK_HEIGHT;
            }
            else
            {
                if (body.Query.Length == 34 || body.Query.Length == 42)
                {
                    response.Found = true;
                    response.Type = EntityType.ADDRESS;
                }
                else if (_utilityService.VerifyHex(body.Query))
                {
                    var tx = await _transactionsRepository.ProbeTransactionByHashAsync(body.Query);
                    if (tx != null)
                    {
                        response.Found = true;
                        response.Type = EntityType.TRANSACTION_HASH;
                    }
                    else
                    {
                        var block = await _blocksRepository.ProbeBlockByHashAsync(body.Query);
                        if (block != null)
                        {
                            response.Found = true;
                            response.Type = EntityType.BLOCK_HASH;
                        }
                    }
                }
            }
        }

        return Ok(response);
    }
}
