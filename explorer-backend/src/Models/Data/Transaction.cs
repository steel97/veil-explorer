namespace ExplorerBackend.Models.Data;

public class Transaction
{
    public string? txid_hex { get; set; }
    public string? hash_hex { get; set; }
    public int version { get; set; }
    public int size { get; set; }
    public int vsize { get; set; }
    public int weight { get; set; }
    public long locktime { get; set; }
    public int block_height { get; set; }
}