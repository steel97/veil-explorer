/* this code is partial port of cpp original code */
using System.Text;

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
public class VeilChainParams
{
    public List<byte[]>? base58Prefixes = [];
    public List<byte[]>? bech32Prefixes = [];

    public byte[] bech32_hrp_stealth = "sv"u8.ToArray();
    public byte[] bech32_hrp_base = "bv"u8.ToArray();

    /*
     consensus.nProgPowTargetSpacing = 172;
     consensus.nRandomXTargetSpacing = 600;
     consensus.nSha256DTargetSpacing = 1200;
    */
    public const int nProgPowTargetSpacing = 172;
    public const int nRandomXTargetSpacing = 600;
    public const int nSha256DTargetSpacing = 1200;


    public VeilChainParams()
    {
        for (var i = 0; i < 14; i++) base58Prefixes.Add([]);
        for (var i = 0; i < 14; i++) bech32Prefixes.Add([]);

        base58Prefixes[(int)Base58Type.PUBKEY_ADDRESS] = [1, 70];
        base58Prefixes[(int)Base58Type.SCRIPT_ADDRESS] = [1, 5];
        base58Prefixes[(int)Base58Type.SECRET_KEY] = [1, 128];
        base58Prefixes[(int)Base58Type.STEALTH_ADDRESS] = [0x84]; // v
        base58Prefixes[(int)Base58Type.EXT_PUBLIC_KEY] = [0x04, 0x88, 0xB2, 0x1E];
        base58Prefixes[(int)Base58Type.EXT_SECRET_KEY] = [0x04, 0x88, 0xAD, 0xE4];

        bech32Prefixes[(int)Base58Type.STEALTH_ADDRESS] = bech32_hrp_stealth;
        bech32Prefixes[(int)Base58Type.BASE_ADDRESS] = bech32_hrp_base;
    }

    public byte[] Base58Prefix(Base58Type type)
    {
        return base58Prefixes![(int)type];
    }
    public byte[] Bech32Prefix(Base58Type type)
    {
        return bech32Prefixes![(int)type];
    }

    public string Bech32HRPStealth() => Encoding.ASCII.GetString(bech32_hrp_stealth);
    public string Bech32HRPBase() => Encoding.ASCII.GetString(bech32_hrp_base);
}