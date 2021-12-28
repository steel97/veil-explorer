namespace ExplorerBackend.VeilStructs;

public enum Base58Type : int
{
    PUBKEY_ADDRESS,
    SCRIPT_ADDRESS,
    SECRET_KEY,
    EXT_PUBLIC_KEY,
    EXT_SECRET_KEY,

    STEALTH_ADDRESS,
    BASE_ADDRESS, //Used in Bech32
    EXT_KEY_HASH,
    EXT_ACC_HASH,
    EXT_PUBLIC_KEY_BTC,
    EXT_SECRET_KEY_BTC,
    PUBKEY_ADDRESS_256,
    SCRIPT_ADDRESS_256,
    STAKE_ONLY_PKADDR,
    MAX_BASE58_TYPES
};

// hard coded for mainnet
public class ChainParams
{
    public List<byte[]>? base58Prefixes = new();
    public List<byte[]>? bech32Prefixes = new();

    public byte[] bech32_hrp_stealth = new byte[] { (byte)'s', (byte)'v' };
    public byte[] bech32_hrp_base = new byte[] { (byte)'b', (byte)'v' };

    public ChainParams()
    {
        for (var i = 0; i < 14; i++) base58Prefixes.Add(new byte[] { });
        for (var i = 0; i < 14; i++) bech32Prefixes.Add(new byte[] { });

        base58Prefixes[(int)Base58Type.PUBKEY_ADDRESS] = new byte[] { 1, 70 };
        base58Prefixes[(int)Base58Type.SCRIPT_ADDRESS] = new byte[] { 1, 5 };
        base58Prefixes[(int)Base58Type.SECRET_KEY] = new byte[] { 1, 128 };
        base58Prefixes[(int)Base58Type.STEALTH_ADDRESS] = new byte[] { 0x84 }; // v
        base58Prefixes[(int)Base58Type.EXT_PUBLIC_KEY] = new byte[] { 0x04, 0x88, 0xB2, 0x1E };
        base58Prefixes[(int)Base58Type.EXT_SECRET_KEY] = new byte[] { 0x04, 0x88, 0xAD, 0xE4 };

        bech32Prefixes[(int)Base58Type.STEALTH_ADDRESS] = new byte[] { (byte)'s', (byte)'v' };
        bech32Prefixes[(int)Base58Type.BASE_ADDRESS] = new byte[] { (byte)'b', (byte)'v' };
    }

    public byte[] Base58Prefix(Base58Type type)
    {
        return base58Prefixes![(int)type];
    }
    public byte[] Bech32Prefix(Base58Type type)
    {
        return bech32Prefixes![(int)type];
    }
}