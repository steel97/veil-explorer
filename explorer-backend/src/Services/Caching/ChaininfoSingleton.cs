using ExplorerBackend.Models.API;
using ExplorerBackend.Models.Node.Response;

namespace ExplorerBackend.Services.Caching;

public class ChaininfoSingleton
{
    public int CurrentSyncedBlock { get; set; }
    public int LastSyncedBlockOnNode { get; set; }
    public GetBlockchainInfoResult? CurrentChainInfo { get; set; }
    public GetChainalgoStatsResult? CurrentChainAlgoStats { get; set; }
    public List<GetRawTransactionResult>? UnconfirmedTxs { get; set; }
    public TxStatsComposite? CurrentChainStats { get; set; }

    public double BudgetWalletAmount { get; set; }
    public double FoundationWalletAmmount { get; set; }

    public SemaphoreSlim BlockchainDataSemaphore = new(1, 1);
    public bool BlockchainDataShouldBroadcast = false;
}