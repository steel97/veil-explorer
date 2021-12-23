namespace explorer_backend.Models.Data;

public class RingctInput
{
    public Guid id { get; set; }
    public Guid tx_input_id { get; set; }
    public string? txid_hex { get; set; }
    public long vout_n { get; set; }
}