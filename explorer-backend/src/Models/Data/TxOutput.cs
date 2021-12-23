namespace explorer_backend.Models.Data;

public class TxOutput
{
    public Guid id { get; set; }
    public string? txid_hex { get; set; }
    public int output_index { get; set; }
    public TxOutputType type { get; set; }
    public long valuesat { get; set; }
    public int vout_n { get; set; }
    public string? scriptpub_asm { get; set; }
    public string? scriptpub_hex { get; set; }
    public TxScriptPubType scriptpub_type { get; set; }
    public int reqsigs { get; set; }
    public List<string>? addresses { get; set; }
    public string? data_hex { get; set; }
    public string? ct_fee { get; set; }
    public string? valuecommitment_hex { get; set; }
    public string? pubkey_hex { get; set; }
}

public enum TxOutputType : short
{
    UNKNOWN = 0,
    COINBASE = 1,
    STANDARD = 2,
    DATA = 3,
    BLIND = 4,
    RINGCT = 5
}

public enum TxScriptPubType : short
{
    UNKNOWN = 0,
    TX_NONSTANDARD = 1,
    TX_PUBKEY = 2,
    TX_PUBKEYHASH = 3,
    TX_PUBKEYHASH256 = 4,
    TX_TIMELOCKED_PUBKEYHASH = 5,
    TX_TIMELOCKED_PUBKEYHASH256 = 6,
    TX_SCRIPTHASH = 7,
    TX_SCRIPTHASH256 = 8,
    TX_TIMELOCKED_SCRIPTHASH = 9,
    TX_TIMELOCKED_SCRIPTHASH256 = 10,
    TX_MULTISIG = 11,
    TX_TIMELOCKED_MULTISIG = 12,
    TX_NULL_DATA = 13,
    TX_WITNESS_V0_KEYHASH = 14,
    TX_WITNESS_V0_SCRIPTHASH = 15,
    TX_WITNESS_UNKNOWN = 16,
    TX_ZEROCOINMINT = 17
}