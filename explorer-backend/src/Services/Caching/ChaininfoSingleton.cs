using ExplorerBackend.Models.Node.Response;

namespace ExplorerBackend.Services.Caching;

public class ChaininfoSingleton
{
    public int currentSyncedBlock { get; set; }
    public GetBlockchainInfoResult? currentChainInfo { get; set; }
    public GetChainalgoStatsResult? currentChainAlgoStats { get; set; }
    public List<GetRawTransactionResult>? UnconfirmedTxs { get; set; }

    public double BudgetWalletAmount { get; set; }
    public double FoundationWalletAmmount { get; set; }
}