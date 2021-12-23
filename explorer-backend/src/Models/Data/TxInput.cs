namespace explorer_backend.Models.Data;

public class TxInput
{
    public Guid id { get; set; }
    public string? txid_hex { get; set; }
    public int input_index { get; set; }
    public TxInputType type { get; set; }
    public int num_inputs { get; set; }
    public int ring_size { get; set; }
    public string? prev_txid_hex { get; set; }
    public string? denomination { get; set; }
    public string? serial_hex { get; set; }
    public string? pubcoin_hex { get; set; }
    public long vout { get; set; }
    public string? scriptsig_asm { get; set; }
    public string? scriptsig_hex { get; set; }
    public List<string>? txinwitnes_hexes { get; set; }
    public long sequence { get; set; }
}

public enum TxInputType : short
{
    UNKNOWN = 0,
    ANON = 1,
    ZEROCOINSPEND = 2
}