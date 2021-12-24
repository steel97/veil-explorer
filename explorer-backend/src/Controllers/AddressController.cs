using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using explorer_backend.Models.API;
using explorer_backend.Configs;
using explorer_backend.Services.Core;
using explorer_backend.Persistence.Repositories;

namespace explorer_backend.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Produces("application/json")]
public class AddressController : ControllerBase
{

    private readonly ILogger _logger;
    private readonly IOptions<APIConfig> _apiConfig;
    private readonly IBlocksRepository _blocksRepository;
    private readonly IUtilityService _utilityService;

    public AddressController(ILogger<AddressController> logger, IOptions<APIConfig> apiConfig, IBlocksRepository blocksRepository, IUtilityService utilityService)
    {
        _logger = logger;
        _apiConfig = apiConfig;
        _blocksRepository = blocksRepository;
        _utilityService = utilityService;
    }

    [HttpPost(Name = "Address")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AddressResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(AddressRequest body)
    {
        var response = new AddressResponse();

        return Ok(response);
    }
}
