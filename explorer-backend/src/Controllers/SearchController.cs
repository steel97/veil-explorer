using Microsoft.AspNetCore.Mvc;
using ExplorerBackend.Models.API;
using ExplorerBackend.Services.Core;
using ExplorerBackend.Services.Caching;
using ExplorerBackend.Persistence.Repositories;

namespace ExplorerBackend.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Produces("application/json")]
public class SearchController : ControllerBase
{
    private readonly ILogger _logger;
    private readonly IBlocksRepository _blocksRepository;
    private readonly ITransactionsRepository _transactionsRepository;
    private readonly ChaininfoSingleton _chaininfoSingleton;
    private readonly IUtilityService _utilityService;

    public SearchController(ILogger<SearchController> logger, IBlocksRepository blocksRepository, ITransactionsRepository transactionsRepository, ChaininfoSingleton chaininfoSingleton, IUtilityService utilityService)
    {
        _logger = logger;
        _blocksRepository = blocksRepository;
        _transactionsRepository = transactionsRepository;
        _chaininfoSingleton = chaininfoSingleton;
        _utilityService = utilityService;
    }

    [HttpPost(Name = "Search")]
    [ProducesResponseType(typeof(SearchResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(SearchRequest body)
    {
        var response = new SearchResponse();
        response.Found = false;
        response.Type = EntityType.UNKNOWN;
        response.Query = body.Query;

        if (body.Query != null)
        {
            if (_utilityService.IsNumeric(body.Query))
            {
                response.Found = true;
                response.Type = EntityType.BLOCK_HEIGHT;
            }
            else
            {
                if (_utilityService.VerifyAddress(body.Query))
                {
                    response.Found = true;
                    response.Type = EntityType.ADDRESS;
                }
                else if (body.Query.Length == 64 && _utilityService.VerifyHex(body.Query))
                {
                    var probeTx = _chaininfoSingleton.UnconfirmedTxs?.Where(tx => tx.txid == body.Query).FirstOrDefault();
                    if (probeTx != null)
                    {
                        response.Found = true;
                        response.Type = EntityType.TRANSACTION_HASH;
                        return Ok(response);
                    }

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
