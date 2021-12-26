using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using ExplorerBackend.Configs;
using ExplorerBackend.Models.API;
using ExplorerBackend.Services.Caching;

namespace ExplorerBackend.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Produces("application/json")]
public class GetMoneySupplyController : ControllerBase
{

    private readonly ILogger _logger;
    private readonly IOptions<ExplorerConfig> _apiConfig;
    private readonly ChaininfoSingleton _chainInfoSingleton;

    public GetMoneySupplyController(ILogger<GetMoneySupplyController> logger, IOptions<ExplorerConfig> apiConfig, ChaininfoSingleton chainInfoSingleton)
    {
        _logger = logger;
        _apiConfig = apiConfig;
        _chainInfoSingleton = chainInfoSingleton;
    }

    [HttpGet(Name = "GetMoneySupplyController")]
    [ProducesResponseType(typeof(MoneySupplyResponse), StatusCodes.Status200OK)]
    public MoneySupplyResponse? Get()
    {
        var totalSupply = _chainInfoSingleton.currentChainInfo?.Moneysupply ?? 0;
        var circulatingSupply = (double)totalSupply - (_chainInfoSingleton.BudgetWalletAmount + _chainInfoSingleton.FoundationWalletAmmount);

        return new MoneySupplyResponse
        {
            total_supply = totalSupply,
            circulating_supply = circulatingSupply,
            team_budget = _chainInfoSingleton.BudgetWalletAmount,
            foundation_budget = _chainInfoSingleton.FoundationWalletAmmount,
            budget_address = _apiConfig.Value.BudgetAddress,
            foundation_address = _apiConfig.Value.FoundationAddress
        };
    }
}
