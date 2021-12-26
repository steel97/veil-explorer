using explorer_backend.Models.Node.Response;

namespace explorer_backend.Services.Caching;

public class ChaininfoSingleton
{
    public int currentSyncedBlock { get; set; }
    public GetBlockchainInfoResult? currentChainInfo { get; set; }
    public GetChainalgoStatsResult? currentChainAlgoStats { get; set; }

    public double BudgetWalletAmount { get; set; }
    public double FoundationWalletAmmount { get; set; }
}