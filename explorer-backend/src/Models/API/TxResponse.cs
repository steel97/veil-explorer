namespace ExplorerBackend.Models.API;

public class TxResponse
{
    public string? TxId { get; set; }
    public bool Confirmed { get; set; }
    public int BlockHeight { get; set; }
    public long Timestamp { get; set; }
    public int Version { get; set; }
    public int Size { get; set; }
    public int VSize { get; set; }
    public long Locktime { get; set; }
    public TransactionSimpleDecoded? Transaction { get; set; }
}