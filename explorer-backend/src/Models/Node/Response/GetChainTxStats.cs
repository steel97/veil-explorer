namespace ExplorerBackend.Models.Node.Response;

public class GetChainTxStats : JsonRPCResponse
{
    public GetChainTxStatsResult? Result { get; set; }
}

public class GetChainTxStatsResult
{
    public long time { get; set; }
    public long txcount { get; set; }
    public string? window_final_block_hash { get; set; }
    public int window_block_count { get; set; }
    public int window_tx_count { get; set; }
    public long window_interval { get; set; }
    public double txrate { get; set; }
}