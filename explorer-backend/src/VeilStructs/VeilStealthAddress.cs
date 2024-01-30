using NBitcoin;

namespace ExplorerBackend.VeilStructs;

public struct stealth_prefix
{
    public uint number_bits;
    public uint bitfield;
};

public class VeilStealthAddress : IDestination
{
    const uint MAX_STEALTH_NARRATION_SIZE = 48;
    const uint MIN_STEALTH_RAW_SIZE = 1 + 33 + 1 + 33 + 1 + 1;
    const uint EC_SECRET_SIZE = 32;
    const uint EC_COMPRESSED_SIZE = 33;
    const uint EC_UNCOMPRESSED_SIZE = 65;

    public uint options = 0;
    public stealth_prefix prefix = new stealth_prefix();
    int number_signatures = 0;
    byte[] scan_pubkey = [];//ec_point
    byte[] spend_pubkey = [];//ec_point
    public byte[]? RawData { get; set; }




    public NBitcoin.Script ScriptPubKey
    {
        get
        {
            return new NBitcoin.Script();
        }
    }

    public int FromRaw(byte[] p)
    {
        RawData = p;
        var nSize = p.Length;
        if (nSize < MIN_STEALTH_RAW_SIZE)
            return 1;
        var index = 0;
        options = p[index++];

        //scan_pubkey.resize(EC_COMPRESSED_SIZE);
        //memcpy(&scan_pubkey[0], p, EC_COMPRESSED_SIZE);
        scan_pubkey = new byte[EC_COMPRESSED_SIZE];
        Array.Copy(p, index, scan_pubkey, 0, EC_COMPRESSED_SIZE);

        index += (int)EC_COMPRESSED_SIZE;
        var spend_pubkeys = p[index++];

        if (nSize < MIN_STEALTH_RAW_SIZE + EC_COMPRESSED_SIZE * (spend_pubkeys - 1))
            return 1;

        //spend_pubkey.resize(EC_COMPRESSED_SIZE * spend_pubkeys);
        //memcpy(&spend_pubkey[0], p, EC_COMPRESSED_SIZE * spend_pubkeys);
        spend_pubkey = new byte[EC_COMPRESSED_SIZE * spend_pubkeys];
        Array.Copy(p, index, spend_pubkey, 0, EC_COMPRESSED_SIZE * spend_pubkeys);


        index += (int)EC_COMPRESSED_SIZE * spend_pubkeys;

        number_signatures = p[index++];
        prefix.number_bits = p[index++];
        prefix.bitfield = 0;
        var nPrefixBytes = Math.Ceiling(prefix.number_bits / 8.0f);

        if (nSize < MIN_STEALTH_RAW_SIZE + EC_COMPRESSED_SIZE * (spend_pubkeys - 1) + nPrefixBytes)
            return 1;

        if (nPrefixBytes >= 1)
            prefix.bitfield = BitConverter.ToUInt32(p.Skip(index).Take((int)nPrefixBytes).ToArray());
        //  memcpy(&prefix.bitfield, p, nPrefixBytes);

        return 0;
    }
}