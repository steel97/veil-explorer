namespace ExplorerBackend.Models.System;

public class ValidateAddress
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
    public uint? prefix_num_bits { get; set; }
    public string? prefix_bitfield { get; set; }
}