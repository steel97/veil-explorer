namespace ExplorerBackend.Models.Node.Response;

public class GetBlockchainInfo : JsonRPCResponse
{
    public GetBlockchainInfoResult? Result { get; set; }
}

public class GetBlockchainInfoResult
{
    public string? Chain { get; set; }
    public uint Blocks { get; set; }
    public ulong Moneysupply { get; set; }
    public List<ZerocoinSupply>? Zerocoinsupply { get; set; }
    public string? Bestblockhash { get; set; }
    public double Difficulty_pow { get; set; }
    public double Difficulty_randomx { get; set; }
    public double Difficulty_progpow { get; set; }
    public double Difficulty_sha256d { get; set; }
    public double Difficulty_pos { get; set; }
    public ulong Mediantime { get; set; }
    public ulong Size_on_disk { get; set; }
    public uint Next_super_block { get; set; }
}

public class ZerocoinSupply
{
    public string? Denom { get; set; }
    public ulong Amount { get; set; }
    public double Percent { get; set; }
}