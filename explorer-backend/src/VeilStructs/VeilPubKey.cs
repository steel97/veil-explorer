using NBitcoin;
using NBitcoin.Crypto;

namespace ExplorerBackend.VeilStructs;

public class VeilPubKey(byte[] buf)
{
    public const uint PUBLIC_KEY_SIZE = 65;
    public const uint COMPRESSED_PUBLIC_KEY_SIZE = 33;
    public const uint SIGNATURE_SIZE = 72;
    public const uint COMPACT_SIGNATURE_SIZE = 65;

    private byte[] _buf = buf;

    public KeyId GetID() => new(Hashes.Hash160(_buf));


    public static uint GetLen(byte chHeader)
    {
        if (chHeader == 2 || chHeader == 3)
            return COMPRESSED_PUBLIC_KEY_SIZE;
        if (chHeader == 4 || chHeader == 6 || chHeader == 7)
            return PUBLIC_KEY_SIZE;
        return 0;
    }

    public static bool ValidSize(byte[] vch) => vch.Length > 0 && GetLen(vch[0]) == vch.Length;
}