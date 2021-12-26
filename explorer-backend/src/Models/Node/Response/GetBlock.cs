namespace ExplorerBackend.Models.Node.Response;

public class GetBlock : JsonRPCResponse
{
    public GetBlockResult? Result { get; set; }
}

public class GetBlockResult
{
    public string? Hash { get; set; }
    public int Confirmations { get; set; }
    public int Strippedsize { get; set; }
    public int Size { get; set; }
    public int Weight { get; set; }
    public int Height { get; set; }
    public string? Proof_type { get; set; }
    public string? Proofofstakehash { get; set; }
    public int Version { get; set; }
    public string? VersionHex { get; set; }
    public string? Merkleroot { get; set; }
    public List<string>? Tx { get; set; }
    public long Time { get; set; }
    public long Mediantime { get; set; }
    public ulong Nonce { get; set; }
    public ulong Nonce64 { get; set; }
    public string? Mixhash { get; set; }
    public string? Bits { get; set; }
    public double Difficulty { get; set; }
    public string? Chainwork { get; set; }
    public int NTx { get; set; }
    public int Anon_index { get; set; }
    public string? Veil_data_hash { get; set; }
    public string? Previousblockhash { get; set; }
    public string? Nextblockhash { get; set; }

    public int epoch_number { get; set; }
    public string? prog_header { get; set; }
    public string? prog_header_hash { get; set; }
    public string? progpowmixhash { get; set; }
    public string? progproofofworkhash { get; set; }
    public string? proofofworkhash { get; set; }
    public string? randomxproofofworkhash { get; set; }
    public string? sha256dproofofworkhash { get; set; }
}