using NBitcoin;
using NBitcoin.DataEncoders;

namespace ExplorerBackend.VeilStructs;

public class Converters
{
    public const int WITNESS_V0_SCRIPTHASH_SIZE = 32;
    public const int WITNESS_V0_KEYHASH_SIZE = 20;

    const int HashSize = 160 / 8;//160/8?

    // piece of bech32, did that for veil specific stuff, can be replaced with NBitcoin implemention?
    static int[] CHARSET_REV = [
    -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    15, -1, 10, 17, 21, 20, 26, 30,  7,  5, -1, -1, -1, -1, -1, -1,
    -1, 29, -1, 24, 13, 25,  9,  8, 23, -1, 18, 22, 31, 27, 19, -1,
     1,  0,  3, 16, 11, 28, 12, 14,  6,  4,  2, -1, -1, -1, -1, -1,
    -1, 29, -1, 24, 13, 25,  9,  8, 23, -1, 18, 22, 31, 27, 19, -1,
     1,  0,  3, 16, 11, 28, 12, 14,  6,  4,  2, -1, -1, -1, -1, -1
];
    bool VerifyChecksum(string hrp, byte[] values)
    {
        // PolyMod computes what value to xor into the final values to make the checksum 0. However,
        // if we required that the checksum was 0, it would be the case that appending a 0 to a valid
        // list of values would result in a new valid list. For that reason, Bech32 requires the
        // resulting checksum to be 1 instead.
        return PolyMod(Cat(ExpandHRP(hrp), values)) == 1;
    }

    byte[] ExpandHRP(string hrp)
    {
        //byte[] ret = new byte[hrp.Length + 90];
        var ret = new byte[hrp.Length * 2 + 1];
        //ret.reserve(hrp.Length + 90);
        //ret.resize(hrp.Length * 2 + 1);
        for (var i = 0; i < hrp.Length; ++i)
        {
            var c = hrp[i];
            ret[i] = (byte)(c >> 5);
            ret[i + hrp.Length + 1] = (byte)(c & 0x1f);
        }
        ret[hrp.Length] = 0;
        return ret;
    }

    public byte[] Cat(byte[] x, byte[] y)
    {
        //x.insert(x.end(), y.begin(), y.end());
        var local = new List<byte>(x);
        local.AddRange(y);
        return local.ToArray();
    }

    private static readonly uint[] Generator = [0x3b6a57b2U, 0x26508e6dU, 0x1ea119faU, 0x3d4233ddU, 0x2a1462b3U];
    private uint PolyMod(byte[] values)
    {
        uint chk = 1;
        for (int i = 0; i < values.Length; i++)
        {
            var top = chk >> 25;
            chk = values[i] ^ ((chk & 0x1ffffff) << 5);
            chk ^= ((top >> 0) & 1) == 1 ? Generator[0] : 0;
            chk ^= ((top >> 1) & 1) == 1 ? Generator[1] : 0;
            chk ^= ((top >> 2) & 1) == 1 ? Generator[2] : 0;
            chk ^= ((top >> 3) & 1) == 1 ? Generator[3] : 0;
            chk ^= ((top >> 4) & 1) == 1 ? Generator[4] : 0;
        }
        return chk;
    }

    public Tuple<string?, byte[]?> Decode(string str)
    {
        bool lower = false, upper = false;
        for (var i = 0; i < str.Length; ++i)
        {
            char c = str[i];
            if (c >= 'a' && c <= 'z') lower = true;
            else if (c >= 'A' && c <= 'Z') upper = true;
            else if (c < 33 || c > 126) return new Tuple<string?, byte[]?>(null, null);
        }

        if (lower && upper) return new Tuple<string?, byte[]?>(null, null);
        var pos = str.LastIndexOf('1');
        if (str.Length > 122 || pos == -1 || pos == 0 || pos + 7 > str.Length)
            return new Tuple<string?, byte[]?>(null, null);

        byte[] values = new byte[str.Length - 1 - pos];
        for (var i = 0; i < str.Length - 1 - pos; ++i)
        {
            char c = str[i + pos + 1];
            var rev = CHARSET_REV[c];

            if (rev == -1)
                return new Tuple<string?, byte[]?>(null, null);

            values[i] = (byte)rev;
        }
        string hrp = "";
        for (var i = 0; i < pos; ++i)
            hrp += str[i].ToString().ToLowerInvariant();

        if (!VerifyChecksum(hrp, values))
            return new Tuple<string?, byte[]?>(null, null);

        return new Tuple<string?, byte[]?>(hrp, values.Take(values.Length - 6).ToArray());
    }

    static bool ConvertBits(Action<byte> outfn, byte[] val, int valOffset, int valCount, int frombits, int tobits, bool pad)

    {
        int acc = 0;
        int bits = 0;
        int maxv = (1 << tobits) - 1;
        int max_acc = (1 << (frombits + tobits - 1)) - 1;
        for (int i = valOffset; i < valOffset + valCount; i++)
        {
            acc = ((acc << frombits) | val[i]) & max_acc;
            bits += frombits;
            while (bits >= tobits)
            {
                bits -= tobits;
                outfn((byte)((acc >> bits) & maxv));
            }
        }
        if (pad)
        {
            if (bits != 0)
                outfn((byte)((acc << (tobits - bits)) & maxv));
        }
        else if (bits >= frombits || ((acc << (tobits - bits)) & maxv) != 0)
        {
            return false;
        }
        return true;
    }

    public IDestination? DecodeDestination(string str)
    {
        var chainParams = new VeilChainParams();
        var b58check = new Base58CheckEncoder();
        byte[] data;

        try
        {
            data = b58check.DecodeData(str);

            // base58-encoded Bitcoin addresses.
            // Public-key-hash-addresses have version 0 (or 111 testnet).
            // The data vector contains RIPEMD160(SHA256(pubkey)), where pubkey is the serialized public key.
            byte[] pubkey_prefix = [chainParams.Base58Prefix(Base58Type.PUBKEY_ADDRESS)[1]];
            if (data.Length == HashSize + pubkey_prefix.Length && data.Take(pubkey_prefix.Length).SequenceEqual(pubkey_prefix)) //std::equal(pubkey_prefix.begin(), pubkey_prefix.end(), data.begin()))
                return new KeyId(data.Skip(pubkey_prefix.Length).ToArray());

            // Script-hash-addresses have version 5 (or 196 testnet).
            // The data vector contains RIPEMD160(SHA256(cscript)), where cscript is the serialized redemption script.
            byte[] script_prefix = [chainParams.Base58Prefix(Base58Type.SCRIPT_ADDRESS)[1]];
            if (data.Length == HashSize + script_prefix.Length && data.Take(script_prefix.Length).SequenceEqual(script_prefix))
                return new ScriptId(data.Skip(script_prefix.Length).ToArray());

            byte[] stealth_prefix = chainParams.Base58Prefix(Base58Type.STEALTH_ADDRESS);
            if (data.Length > stealth_prefix.Length && data.Take(stealth_prefix.Length).SequenceEqual(stealth_prefix))
            {
                var sx = new VeilStealthAddress();
                if (0 == sx.FromRaw(data.Skip(stealth_prefix.Length).ToArray()))
                    return sx;
                return null;
            }
        }
        catch
        {

        }


        data = [];
        var dataz = new List<byte>();
        var bech = Decode(str);

        if (bech != null && bech.Item2 != null && bech.Item2.Length > 0 && (bech.Item1 == chainParams.Bech32HRPStealth() || bech.Item1 == chainParams.Bech32HRPBase()))
        {
            // Bech32 decoding
            int version = bech!.Item2![0]; // The first 5 bit symbol is the witness version (0-16)
            bool fIsStealth = bech.Item1 == chainParams.Bech32HRPStealth();
            // The rest of the symbols are converted witness program bytes.
            //if (fIsStealth)
            //    data.reserve(((bech.second.size()) * 5) / 8);
            //else
            //    data.reserve(((bech.second.size() - 1) * 5) / 8);

            var offsetl = fIsStealth ? 0 : 1;
            //ConvertBits(c => data.Add(c), bech.Item2, offsetl, bech.Item2.Count() - offsetl, 5, 8, false)
            //if (ConvertBits < 5, 8, false > ([&](unsigned char c) { data.push_back(c); }, bech.second.begin() + (fIsStealth ? 0 : 1), bech.second.end()))
            if (ConvertBits(c => dataz.Add(c), bech.Item2, offsetl, bech.Item2.Length - offsetl, 5, 8, false))
            {
                data = dataz.ToArray();
                if (version == 0)
                {
                    {


                        if (data.Length == WITNESS_V0_KEYHASH_SIZE)
                            return new WitKeyId(data);
                    }
                    {
                        if (data.Length == WITNESS_V0_SCRIPTHASH_SIZE)
                            return new WitScriptId(data);
                    }
                    {
                        if (data.Length == 70)
                        { //Size of stealth addr
                            var sx = new VeilStealthAddress();
                            if (0 == sx.FromRaw(data))
                                return sx;
                        }
                    }
                    return null;
                }
                if (version > 16 || data.Length < 2 || data.Length > 40)
                    return null;

                var unk = new VeilWitnessUnknown
                {
                    version = version,
                    program = data
                };

                return unk;
            }
        }
        return null;
    }

    public bool IsValidDestination(IDestination dest)
    {
        //CKeyID, CScriptID, WitnessV0ScriptHash, WitnessV0KeyHash, WitnessUnknown, CStealthAddress, CExtKeyPair, CKeyID256, CScriptID256
        return dest is KeyId || dest is ScriptId || dest is WitScriptId || dest is WitKeyId || dest is VeilStealthAddress || dest is ExtKey || dest is VeilWitnessUnknown;
    }

    public static string EncodeDestination(IDestination value, bool m_bech32 = false)
    {
        var m_params = new VeilChainParams();
        var b58check = new Base58CheckEncoder();
        if (value is KeyId)
        {
            /*if (m_bech32)
            {
                var vchVersion = m_params.Bech32Prefix(CChainParams::PUBKEY_ADDRESS);
                std::string sHrp(vchVersion.begin(), vchVersion.end());
                std::vector < unsigned char> data;
                data.reserve(32);
                ConvertBits < 8, 5, true > ([&](unsigned char c) { data.push_back(c); }, id.begin(), id.end());
                return bech32::Encode(sHrp, data);
                var benc = new Bech32Encoder();
                benc.Encode()
            }*/

            var data = new List<byte>
            {
                m_params.Base58Prefix(Base58Type.PUBKEY_ADDRESS)[1]
            };

            var id = (KeyId)value;

            data.AddRange(id.ToBytes());
            return b58check.EncodeData(data.ToArray());
        }
        if (value is ScriptId)
        {
            /*if (m_bech32)
            {
                const auto &vchVersion = m_params.Bech32Prefix(CChainParams::SCRIPT_ADDRESS);
                std::string sHrp(vchVersion.begin(), vchVersion.end());
                std::vector < unsigned char> data;
                ConvertBits < 8, 5, true > ([&](unsigned char c) { data.push_back(c); }, id.begin(), id.end());
                return bech32::Encode(sHrp, data);
            }*/

            var data = new List<byte>
            {
                m_params.Base58Prefix(Base58Type.SCRIPT_ADDRESS)[1]
            };

            var id = (ScriptId)value;

            data.AddRange(id.ToBytes());
            return b58check.EncodeData(data.ToArray());
        }

        if (value is WitKeyId id2)
        {
            var bech = new Bech32Encoder(m_params.bech32_hrp_base);
            /*var data = new List<byte>(new byte[]{ 0x00 });
            data.reserve(33);
            ConvertBits < 8, 5, true > ([&](unsigned char c) { data.push_back(c); }, id.begin(), id.end());
            return bech32::Encode(m_params.Bech32HRPBase(), data);*/
            var id = id2;
            return bech.Encode(0, id.ToBytes());
        }

        if (value is VeilStealthAddress address)
        {
            var bech = new Bech32Encoder(m_params.bech32_hrp_stealth);
            var id = address;
            return bech.Encode(0, id.RawData);
        }

        if (value is WitScriptId id1)
        {
            var bech = new Bech32Encoder(m_params.bech32_hrp_base);
            /*std::vector < unsigned char> data = { 0};
            data.reserve(53);
            ConvertBits < 8, 5, true > ([&](unsigned char c) { data.push_back(c); }, id.begin(), id.end());*/

            var id = id1;
            return bech.Encode(0, id.ToBytes());
        }
        else
        {
            // unknown?
            var bech = new Bech32Encoder(null);
            var id = (WitScriptId)value;
            return bech.Encode(0, id.ToBytes());
        }
    }

    public static int DecodeOP_N(opcodetype opcode)
    {
        if (opcode == opcodetype.OP_0)
            return 0;
        if (!(opcode >= opcodetype.OP_1 && opcode <= opcodetype.OP_16))
            throw new Exception("DecodeOP_N");
        return (int)opcode - (int)(opcodetype.OP_1 - 1);
    }

    public static bool Solver(Script scriptPubKey, out txnouttype typeRet, List<byte[]>? vSolutionsRet)
    {
        vSolutionsRet?.Clear();

        // Shortcut for pay-to-script-hash, which are more constrained than the other types:
        // it is always OP_HASH160 20 [20 byte hash] OP_EQUAL
        if (scriptPubKey.IsPayToScriptHash())
        {
            typeRet = txnouttype.TX_SCRIPTHASH;
            byte[] hashBytes = new byte[20];
            Array.Copy(scriptPubKey.Hash!, 2, hashBytes, 0, hashBytes.Length);
            vSolutionsRet?.Add(hashBytes);
            return true;
        }

        if (scriptPubKey.IsWitnessProgram(out int witnessversion, out byte[] witnessprogram))
        {
            if (witnessversion == 0 && witnessprogram.Length == WITNESS_V0_KEYHASH_SIZE)
            {
                typeRet = txnouttype.TX_WITNESS_V0_KEYHASH;
                vSolutionsRet?.Add(witnessprogram);
                return true;
            }
            if (witnessversion == 0 && witnessprogram.Length == WITNESS_V0_SCRIPTHASH_SIZE)
            {
                typeRet = txnouttype.TX_WITNESS_V0_SCRIPTHASH;
                vSolutionsRet?.Add(witnessprogram);
                return true;
            }
            if (witnessversion != 0)
            {
                typeRet = txnouttype.TX_WITNESS_UNKNOWN;
                vSolutionsRet?.Add([(byte)witnessversion]);
                vSolutionsRet?.Add(witnessprogram);
                return true;
            }
            typeRet = txnouttype.TX_NONSTANDARD;
            return false;
        }

        // Provably prunable, data-carrying output
        //
        // So long as script passes the IsUnspendable() test and all but the first
        // byte passes the IsPushOnly() test we don't care what exactly is in the
        // script.
        if (scriptPubKey.Hash?.Length >= 1 && scriptPubKey.Hash![0] == (byte)opcodetype.OP_RETURN && scriptPubKey.IsPushOnly(1))
        {
            typeRet = txnouttype.TX_NULL_DATA;
            return true;
        }

        if (MatchPayToPubkey(scriptPubKey, out byte[] data))
        {
            typeRet = txnouttype.TX_PUBKEY;
            vSolutionsRet?.Add(data);
            return true;
        }

        if (MatchPayToPubkeyHash(scriptPubKey, out data))
        {
            typeRet = txnouttype.TX_PUBKEYHASH;
            vSolutionsRet?.Add(data);
            return true;
        }

        List<byte[]> keys = [];
        if (MatchMultisig(scriptPubKey, out uint required, keys))
        {
            typeRet = txnouttype.TX_MULTISIG;
            vSolutionsRet?.Add([(byte)required]); // safe as required is in range 1..16
                                                               //vSolutionsRet.insert(vSolutionsRet.end(), keys.begin(), keys.end());
            foreach (var key in keys)
                vSolutionsRet?.Add(key);

            vSolutionsRet?.Add([(byte)keys.Count]); // safe as size is in range 1..16
            return true;
        }

        if (scriptPubKey.IsZerocoinMint())
        {
            typeRet = txnouttype.TX_ZEROCOINMINT;
            // Zerocoin
            if (scriptPubKey.IsZerocoinMint())
            {
                typeRet = txnouttype.TX_ZEROCOINMINT;
                if (scriptPubKey.Hash?.Length > 150) return false;
                byte[] hashBytes = new byte[(scriptPubKey.Hash?.Length ?? 0) - 2];
                Array.Copy(scriptPubKey.Hash!, 2, hashBytes, 0, hashBytes.Length);
                vSolutionsRet?.Add(hashBytes);
                return true;
            }
            return true;
        }

        vSolutionsRet?.Clear();
        typeRet = txnouttype.TX_NONSTANDARD;
        return false;
    }

    public static bool MatchPayToPubkey(Script script, out byte[] pubkey)
    {
        pubkey = [];
        if (script.Size() == VeilPubKey.PUBLIC_KEY_SIZE + 2 && script.Hash?[0] == VeilPubKey.PUBLIC_KEY_SIZE && script.Hash?.Last() == (byte)opcodetype.OP_CHECKSIG)
        {
            //pubkey = valtype(script.begin() + 1, script.begin() + CPubKey::PUBLIC_KEY_SIZE + 1);
            pubkey = new byte[VeilPubKey.PUBLIC_KEY_SIZE];
            Array.Copy(script.Hash!, 1, pubkey, 0, pubkey.Length);
            return VeilPubKey.ValidSize(pubkey);
        }
        if (script.Size() == VeilPubKey.COMPRESSED_PUBLIC_KEY_SIZE + 2 && script.Hash?[0] == VeilPubKey.COMPRESSED_PUBLIC_KEY_SIZE && script.Hash.Last() == (byte)opcodetype.OP_CHECKSIG)
        {
            //pubkey = valtype(script.begin() + 1, script.begin() + CPubKey::COMPRESSED_PUBLIC_KEY_SIZE + 1);
            pubkey = new byte[VeilPubKey.COMPRESSED_PUBLIC_KEY_SIZE];
            Array.Copy(script.Hash!, 1, pubkey, 0, pubkey.Length);
            return VeilPubKey.ValidSize(pubkey);
        }
        return false;
    }

    public static bool MatchPayToPubkeyHash(Script script, out byte[] pubkeyhash)
    {
        pubkeyhash = [];
        if (script.Size() == 25 && script.Hash?[0] == (byte)opcodetype.OP_DUP && script.Hash?[1] == (byte)opcodetype.OP_HASH160 && script.Hash?[2] == 20 && script.Hash?[23] == (byte)opcodetype.OP_EQUALVERIFY && script.Hash?[24] == (byte)opcodetype.OP_CHECKSIG)
        {
            //pubkeyhash = valtype(script.begin() + 3, script.begin() + 23);
            pubkeyhash = new byte[20];
            Array.Copy(script.Hash, 3, pubkeyhash, 0, pubkeyhash.Length);
            return true;
        }
        return false;
    }

    public static bool IsSmallInteger(opcodetype opcode) => opcode >= opcodetype.OP_1 && opcode <= opcodetype.OP_16;


    public static bool MatchMultisig(Script script, out uint required, List<byte[]> pubkeys)
    {
        opcodetype opcode = opcodetype.OP_0;
        required = 0;
        int it = 0;
        if (script.Size() < 1 || script.Hash?.Last() != (byte)opcodetype.OP_CHECKMULTISIG) return false;

        if (!script.GetOp(ref it, ref opcode, out byte[] data) || !IsSmallInteger(opcode)) return false;

        required = (uint)Script.DecodeOP_N(opcode);
        
        while (script.GetOp(ref it, ref opcode, out data) && VeilPubKey.ValidSize(data))
        {
            pubkeys.Add(data);
        }
        if (!IsSmallInteger(opcode)) return false;
        var keys = (uint)Script.DecodeOP_N(opcode);
        if (pubkeys.Count != keys || keys < required) return false;
        return it + 1 == script.Hash.Length;
    }

    public static bool ExtractDestination(Script scriptPubKey, out IDestination addressRet)
    {
        var vSolutions = new List<byte[]>();
        addressRet = new KeyId();
        if (!Solver(scriptPubKey, out txnouttype whichType, vSolutions))
            return false;

        if (whichType == txnouttype.TX_PUBKEY)
        {
            var pubKey = vSolutions[0];
            //if (!pubKey.IsValid())
            //    return false;

            addressRet = new VeilPubKey(pubKey).GetID();
            return true;
        }
        else if (whichType == txnouttype.TX_PUBKEYHASH)
        {
            addressRet = new KeyId(new uint160(vSolutions[0]));
            return true;
        }
        else if (whichType == txnouttype.TX_SCRIPTHASH)
        {
            addressRet = new ScriptId(new uint160(vSolutions[0]));
            return true;
        }
        else if (whichType == txnouttype.TX_WITNESS_V0_KEYHASH)
        {
            //new WitnessScript
            //WitnessV0KeyHash hash;
            //std::copy(vSolutions[0].begin(), vSolutions[0].end(), hash.begin());
            addressRet = new WitKeyId(vSolutions[0]);
            return true;
        }
        else if (whichType == txnouttype.TX_WITNESS_V0_SCRIPTHASH)
        {
            //WitnessV0ScriptHash hash;
            //std::copy(vSolutions[0].begin(), vSolutions[0].end(), hash.begin());            
            addressRet = new WitScriptId(vSolutions[0]);
            return true;
        }
        else if (whichType == txnouttype.TX_WITNESS_UNKNOWN)
        {
            //WitnessUnknown unk;
            //unk.version = vSolutions[0][0];
            //std::copy(vSolutions[1].begin(), vSolutions[1].end(), unk.program);
            //unk.length = vSolutions[1].Count();
            //addressRet = unk;
            addressRet = new WitKeyId(vSolutions[0]);
            return true;
        }
        // Multisig txns have more than one address...
        return false;
    }

    public static bool ExtractDestinations(Script scriptPubKey, out txnouttype typeRet, List<IDestination> addressRet, out int nRequiredRet)
    {
        addressRet.Clear();

        typeRet = txnouttype.TX_NONSTANDARD;
        nRequiredRet = 0;

        txnouttype ctout;

        List<byte[]> vSolutions = [];

        if (!Solver(scriptPubKey, out ctout, vSolutions))
            return false;

        typeRet = ctout;

        if (typeRet == txnouttype.TX_NULL_DATA)
        {
            // This is data, not addresses
            return false;
        }

        if (typeRet == txnouttype.TX_MULTISIG)
        {
            nRequiredRet = vSolutions[0][0];
            for (uint i = 1; i < vSolutions.Count - 1; i++)
            {
                var pubKey = vSolutions[(int)i];
                //CPubKey pubKey(vSolutions[i]);
                //if (!pubKey.IsValid())
                //    continue;

                //CTxDestination address = pubKey.GetID();
                var address = new VeilPubKey(pubKey).GetID();
                addressRet.Add(address);
            }

            if (addressRet.Count == 0)
                return false;
        }
        else
        {
            nRequiredRet = 1;
            if (!ExtractDestination(scriptPubKey, out IDestination address))
                return false;
            addressRet.Add(address);
        }

        return true;
    }
}

public enum txnouttype
{
    TX_NONSTANDARD,
    // 'standard' transaction types:
    TX_PUBKEY,
    TX_PUBKEYHASH,
    TX_SCRIPTHASH,
    TX_MULTISIG,
    TX_NULL_DATA, //!< unspendable OP_RETURN script that carries data
    TX_WITNESS_V0_SCRIPTHASH,
    TX_WITNESS_V0_KEYHASH,
    TX_WITNESS_UNKNOWN, //!< Only for Witness versions not already defined above

    TX_SCRIPTHASH256,
    TX_PUBKEYHASH256,
    TX_TIMELOCKED_SCRIPTHASH,
    TX_TIMELOCKED_SCRIPTHASH256,
    TX_TIMELOCKED_PUBKEYHASH,
    TX_TIMELOCKED_PUBKEYHASH256,
    TX_TIMELOCKED_MULTISIG,
    TX_ZEROCOINMINT,
}

public enum SigVersion : byte
{
    BASE = 0,
    WITNESS_V0 = 1,
}