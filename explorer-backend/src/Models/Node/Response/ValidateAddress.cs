namespace ExplorerBackend.Models.Node.Response;

public class ValidateAddrees : JsonRPCResponse
{
    public ValidateAddreesResult? Result { get; set; }
}

public class ValidateAddreesResult
{
    public bool isvalid { get; set; }
    public string? address { get; set; }
    public string? scriptPubKey { get; set; }

    public bool? isscript { get; set; }
    public bool? iswitness { get; set; }

    public int? witness_version { get; set; }
    public string? witness_program { get; set; }

    public bool? isextkey { get; set; }

    public bool? isstealthaddress { get; set; }
    public int? prefix_num_bits { get; set; }
    public string? prefix_bitfield { get; set; }
}