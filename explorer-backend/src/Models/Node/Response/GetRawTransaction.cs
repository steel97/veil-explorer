using System.Text.Json.Serialization;

namespace ExplorerBackend.Models.Node.Response;

public class GetRawTransaction : JsonRPCResponse
{
    public GetRawTransactionResult? Result { get; set; }
}

public class GetRawTransactionResult
{
    public bool in_active_chain { get; set; }

    public string? txid { get; set; }
    public string? hash { get; set; }
    public int version { get; set; }
    public int size { get; set; }
    public int vsize { get; set; }
    public int weight { get; set; }
    public long locktime { get; set; }


    public List<TxVin>? vin { get; set; }
    public List<TxVout>? vout { get; set; }


    public string? blockhash { get; set; }
    public string? hex { get; set; }
    public int confirmations { get; set; }
    public long time { get; set; }
    public long blocktime { get; set; }
}

public class TxVin
{
    public string? coinbase { get; set; }
    public string? type { get; set; }
    public int num_inputs { get; set; }
    public int ring_size { get; set; }
    public List<RingctInput>? ringct_inputs { get; set; }

    public string? txid { get; set; }
    public string? denomination { get; set; }
    public string? serial { get; set; }
    public string? pubcoin { get; set; }
    public long vout { get; set; }
    public ScriptSig? scriptSig { get; set; }
    public List<string>? txinwitness { get; set; }
    public long sequence { get; set; }
}

public class RingctInput
{
    public string? txid { get; set; }
    [JsonPropertyName("vout.n")]
    public long vout_n { get; set; }
}

public class ScriptSig
{
    public string? asm { get; set; }
    public string? hex { get; set; }
}

public class TxVout
{
    public int output_index { get; set; }
    public string? type { get; set; }
    public double value { get; set; }
    public long valueSat { get; set; }
    [JsonPropertyName("vout.n")]
    public int vout_n { get; set; }
    public ScriptPubKey? scriptPubKey { get; set; }



    public string? data_hex { get; set; }
    public double ct_fee { get; set; }
    public string? valueCommitment { get; set; }
    public string? pubkey { get; set; }
}

public class ScriptPubKey
{
    public string? asm { get; set; }
    public string? hex { get; set; }
    public string? type { get; set; }
    public int reqSigs { get; set; }
    public List<string>? addresses { get; set; }
}