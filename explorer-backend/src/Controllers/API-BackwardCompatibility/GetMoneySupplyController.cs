using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using ExplorerBackend.VeilStructs;
using ExplorerBackend.Configs;
using ExplorerBackend.Models.API;
using ExplorerBackend.Services.Caching;

namespace ExplorerBackend.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Produces("application/json")]
public class GetMoneySupplyController(IOptions<ExplorerConfig> apiConfig, ChaininfoSingleton chainInfoSingleton) : ControllerBase
{
    private readonly IOptions<ExplorerConfig> _apiConfig = apiConfig;
    private readonly ChaininfoSingleton _chainInfoSingleton = chainInfoSingleton;

    [HttpGet(Name = "GetMoneySupplyController")]
    [ProducesResponseType(typeof(MoneySupplyResponse), StatusCodes.Status200OK)]
    public MoneySupplyResponse? Get()
    {
        var totalSupply = _chainInfoSingleton.CurrentChainInfo?.Moneysupply ?? 0;
        var totalSupplyPrepared = (double)totalSupply / Constants.COIN;
        var circulatingSupply = totalSupplyPrepared - (_chainInfoSingleton.BudgetWalletAmount + _chainInfoSingleton.FoundationWalletAmmount);

        return new MoneySupplyResponse
        {
            total_supply = totalSupplyPrepared,
            circulating_supply = circulatingSupply,
            team_budget = _chainInfoSingleton.BudgetWalletAmount,
            foundation_budget = _chainInfoSingleton.FoundationWalletAmmount,
            budget_address = _apiConfig.Value.BudgetAddress,
            foundation_address = _apiConfig.Value.FoundationAddress
        };
    }
}
