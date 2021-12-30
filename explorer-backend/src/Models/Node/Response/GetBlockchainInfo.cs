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
    public string? Moneysupply_formatted { get; set; } // for compatability
    public List<ZerocoinSupply>? Zerocoinsupply { get; set; }
    public uint Headers { get; set; } // for compatability
    public string? Bestblockhash { get; set; }
    public double Difficulty_pow { get; set; }
    public double Difficulty_randomx { get; set; }
    public double Difficulty_progpow { get; set; }
    public double Difficulty_sha256d { get; set; }
    public double Difficulty_pos { get; set; }
    public ulong Mediantime { get; set; }
    // for compatability
    public double verificationprogress { get; set; }
    public bool initialblockdownload { get; set; }
    public string? chainwork { get; set; }
    public string? chainpow { get; set; }
    // end

    public ulong Size_on_disk { get; set; }
    // for compatability
    public bool pruned { get; set; }
    public Dictionary<string, Softfork>? bip9_softforks { get; set; }
    public string? warnings { get; set; }
    // end
    public uint Next_super_block { get; set; }
}

public class ZerocoinSupply
{
    public string? Denom { get; set; }
    public ulong Amount { get; set; }
    public string? Amount_formatted { get; set; } // for compatability
    public double Percent { get; set; }
}

// for compatability
public class Softfork
{
    public string? status { get; set; }
    public long startTime { get; set; }
    public long timeout { get; set; }
    public uint since { get; set; }
}
// end