namespace ExplorerBackend.Models.Node.Response;

public class ScanTxOutset : JsonRPCResponse
{
    public ScanTxOutsetResult? Result { get; set; }
}

public class ScanTxOutsetResult
{
    public bool success { get; set; }
    public ulong searched_items { get; set; }
    public List<UnspentOutput>? unspents { get; set; }
    public double total_amount { get; set; }
}

public class UnspentOutput
{
    public string? txid { get; set; }
    public int vout { get; set; }
    public string? scriptPubKey { get; set; }
    public double amount { get; set; }
    public int height { get; set; }
}