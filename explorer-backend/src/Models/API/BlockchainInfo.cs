using ExplorerBackend.Models.Node.Response;

namespace ExplorerBackend.Models.API;

public class BlockchainInfo
{
    public int CurrentSyncedBlock { get; set; }
    public GetBlockchainInfoResult? ChainInfo { get; set; }
    public GetChainalgoStatsResult? AlgoStats { get; set; }
    public Dictionary<string, double>? NetworkHashrates { get; set; }
}
