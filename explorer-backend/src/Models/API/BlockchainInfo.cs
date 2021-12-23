using explorer_backend.Models.Node.Response;

namespace explorer_backend.Models.API;

public class BlockchainInfo
{
    public int CurrentSyncedBlock { get; set; }
    public GetBlockchainInfoResult? ChainInfo { get; set; }
    public GetChainalgoStatsResult? AlgoStats { get; set; }
}
