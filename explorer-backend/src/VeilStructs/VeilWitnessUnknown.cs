using NBitcoin;

namespace ExplorerBackend.VeilStructs;

public class VeilWitnessUnknown : IDestination
{
    public int version { get; set; }
    public byte[]? program { get; set; }
    public NBitcoin.Script ScriptPubKey
    {
        get
        {
            return new NBitcoin.Script();
        }
    }
}