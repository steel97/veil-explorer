using ExplorerBackend.Models.Node.Response;

namespace ExplorerBackend.Models.Data;

public class Block
{
    public int height { get; set; }
    public string? hash_hex { get; set; }
    public int strippedsize { get; set; }
    public int size { get; set; }
    public int weight { get; set; }
    public BlockType proof_type { get; set; }
    public string? proofofstakehash_hex { get; set; }
    public string? progproofofworkhash_hex { get; set; }
    public string? progpowmixhash_hex { get; set; }
    public string? randomxproofofworkhash_hex { get; set; }
    public string? sha256dproofofworkhash_hex { get; set; }
    public string? proofofworkhash_hex { get; set; }
    public int version { get; set; }
    public string? merkleroot_hex { get; set; }
    public List<GetRawTransactionResult>? tx { get; set; }
    public long time { get; set; }
    public long mediantime { get; set; }
    public ulong nonce { get; set; }
    public ulong nonce64 { get; set; }
    public string? mixhash_hex { get; set; }
    public string? bits_hex { get; set; }
    public double difficulty { get; set; }
    public string? chainwork_hex { get; set; }
    public long anon_index { get; set; }
    public string? veil_data_hash_hex { get; set; }
    public string? prog_header_hash_hex { get; set; }
    public string? prog_header_hex { get; set; }
    public int epoch_number { get; set; }
    public bool synced { get; set; }
    public int txnCount { get; set; }
}

public enum BlockType : byte
{
    UNKNOWN = 0,
    POW_X16RT = 1,
    POW_ProgPow = 2,
    POW_RandomX = 3,
    POW_Sha256D = 4,
    POS = 5
}