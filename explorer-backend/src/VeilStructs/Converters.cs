using NBitcoin;
using NBitcoin.DataEncoders;

namespace ExplorerBackend.VeilStructs;

public class Converters
{
    public const int WITNESS_V0_SCRIPTHASH_SIZE = 32;
    public const int WITNESS_V0_KEYHASH_SIZE = 20;

    public static string EncodeDestination(IDestination value, bool m_bech32 = false)
    {
        var m_params = new ChainParams();
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

            var data = new List<byte>(m_params.Base58Prefix(Base58Type.PUBKEY_ADDRESS));
            var id = (KeyId)value;

            //data.insert(data.end(), id.begin(), id.end());
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

            var data = new List<byte>(m_params.Base58Prefix(Base58Type.SCRIPT_ADDRESS));
            var id = (ScriptId)value;

            data.AddRange(id.ToBytes());
            return b58check.EncodeData(data.ToArray());
        }
        if (value is WitKeyId)
        {
            var bech = new Bech32Encoder(m_params.bech32_hrp_base);
            /*var data = new List<byte>(new byte[]{ 0x00 });
            data.reserve(33);
            ConvertBits < 8, 5, true > ([&](unsigned char c) { data.push_back(c); }, id.begin(), id.end());
            return bech32::Encode(m_params.Bech32HRPBase(), data);*/
            var id = (WitKeyId)value;
            return bech.Encode(0, id.ToBytes());
        }
        if (value is WitScriptId)
        {
            var bech = new Bech32Encoder(m_params.bech32_hrp_base);
            /*std::vector < unsigned char> data = { 0};
            data.reserve(53);
            ConvertBits < 8, 5, true > ([&](unsigned char c) { data.push_back(c); }, id.begin(), id.end());*/

            var id = (WitScriptId)value;
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
            byte[] hashBytes = new byte[22];
            Array.Copy(scriptPubKey.Hash!, 2, hashBytes, 0, hashBytes.Length);
            vSolutionsRet?.Add(hashBytes);
            return true;
        }

        int witnessversion;
        byte[] witnessprogram;
        if (scriptPubKey.IsWitnessProgram(out witnessversion, out witnessprogram))
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
                vSolutionsRet?.Add(new byte[] { (byte)witnessversion });
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

        byte[] data;
        if (MatchPayToPubkey(scriptPubKey, out data))
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

        uint required;
        List<byte[]> keys = new();
        if (MatchMultisig(scriptPubKey, out required, keys))
        {
            typeRet = txnouttype.TX_MULTISIG;
            vSolutionsRet?.Add(new byte[] { (byte)required }); // safe as required is in range 1..16
            //vSolutionsRet.insert(vSolutionsRet.end(), keys.begin(), keys.end());
            foreach (var key in keys)
                vSolutionsRet?.Add(key);

            vSolutionsRet?.Add(new byte[] { (byte)keys.Count() }); // safe as size is in range 1..16
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
        pubkey = new byte[] { };
        if (script.Size() == VeilPubKey.PUBLIC_KEY_SIZE + 2 && script.Hash?[0] == VeilPubKey.PUBLIC_KEY_SIZE && script.Hash?.Last() == (byte)opcodetype.OP_CHECKSIG)
        {
            //pubkey = valtype(script.begin() + 1, script.begin() + CPubKey::PUBLIC_KEY_SIZE + 1);
            pubkey = new byte[VeilPubKey.PUBLIC_KEY_SIZE + 1];
            Array.Copy(script.Hash!, 1, pubkey, 0, pubkey.Length);
            return VeilPubKey.ValidSize(pubkey);
        }
        if (script.Size() == VeilPubKey.COMPRESSED_PUBLIC_KEY_SIZE + 2 && script.Hash?[0] == VeilPubKey.COMPRESSED_PUBLIC_KEY_SIZE && script.Hash.Last() == (byte)opcodetype.OP_CHECKSIG)
        {
            //pubkey = valtype(script.begin() + 1, script.begin() + CPubKey::COMPRESSED_PUBLIC_KEY_SIZE + 1);
            pubkey = new byte[VeilPubKey.COMPRESSED_PUBLIC_KEY_SIZE + 1];
            Array.Copy(script.Hash!, 1, pubkey, 0, pubkey.Length);
            return VeilPubKey.ValidSize(pubkey);
        }
        return false;
    }

    public static bool MatchPayToPubkeyHash(Script script, out byte[] pubkeyhash)
    {
        pubkeyhash = new byte[] { };
        if (script.Size() == 25 && script.Hash?[0] == (byte)opcodetype.OP_DUP && script.Hash?[1] == (byte)opcodetype.OP_HASH160 && script.Hash?[2] == 20 && script.Hash?[23] == (byte)opcodetype.OP_EQUALVERIFY && script.Hash?[24] == (byte)opcodetype.OP_CHECKSIG)
        {
            //pubkeyhash = valtype(script.begin() + 3, script.begin() + 23);
            pubkeyhash = new byte[23];
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
        byte[] data;
        int it = 0;
        if (script.Size() < 1 || script.Hash?.Last() != (byte)opcodetype.OP_CHECKMULTISIG) return false;

        if (!script.GetOp(ref it, ref opcode, out data) || !IsSmallInteger(opcode)) return false;
        required = (uint)Script.DecodeOP_N(opcode);
        while (script.GetOp(ref it, ref opcode, out data) && VeilPubKey.ValidSize(data))
        {
            pubkeys.Add(data);
        }
        if (!IsSmallInteger(opcode)) return false;
        var keys = (uint)Script.DecodeOP_N(opcode);
        if (pubkeys.Count() != keys || keys < required) return false;
        return (it + 1 == script.Hash.Count());
    }

    public static bool ExtractDestination(Script scriptPubKey, out IDestination addressRet)
    {
        List<byte[]> vSolutions = new List<byte[]>();
        txnouttype whichType;
        addressRet = new KeyId();
        if (!Solver(scriptPubKey, out whichType, vSolutions))
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

        List<byte[]> vSolutions = new List<byte[]>();

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
            for (uint i = 1; i < vSolutions.Count() - 1; i++)
            {
                var pubKey = vSolutions[(int)i];
                //CPubKey pubKey(vSolutions[i]);
                //if (!pubKey.IsValid())
                //    continue;

                //CTxDestination address = pubKey.GetID();
                var address = new VeilPubKey(pubKey).GetID();
                addressRet.Add(address);
            }

            if (addressRet.Count() == 0)
                return false;
        }
        else
        {
            nRequiredRet = 1;
            IDestination address;
            if (!ExtractDestination(scriptPubKey, out address))
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