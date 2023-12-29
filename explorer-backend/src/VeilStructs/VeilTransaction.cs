using NBitcoin;

namespace ExplorerBackend.VeilStructs;

public class VeilTransaction : IVeilSerializable
{
    public static readonly byte[] ZEROHASH = [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

    public int Version { get; set; }
    public uint LockTime { get; set; }
    public List<VeilTxIn>? TxIn { get; set; }
    public List<VeilTxOut>? TxOut { get; set; }

    public void Deserialize(VeilSerialization serializationContext, int mode)
    {
        serializationContext.ReadByte(out byte bv);

        Version = bv;

        serializationContext.ReadByte(out bv);
        Version |= bv << 8;

        serializationContext.ReadBool(out bool fUseSegwit);

        serializationContext.ReadUint(out uint lockTime);
        LockTime = lockTime;

        serializationContext.ReadCompactSize(out ulong vInCount);

        TxIn = [];
        for (var i = 0UL; i < vInCount; i++)
        {
            var txin = new VeilTxIn();
            txin.Deserialize(serializationContext, mode);
            TxIn.Add(txin);
        }

        serializationContext.ReadCompactSize(out ulong vOutCount);

        TxOut = [];
        for (var i = 0UL; i < vOutCount; i++)
        {
            serializationContext.ReadByte(out byte outputType);

            VeilTxOut txout = (OutputTypes)outputType switch
            {
                OutputTypes.OUTPUT_STANDARD => new VeilTxOutStandard(),
                OutputTypes.OUTPUT_CT => new VeilTxOutCT(),
                OutputTypes.OUTPUT_RINGCT => new VeilTxOutRingCT(),
                OutputTypes.OUTPUT_DATA => new VeilTxOutData(),
                _ => throw new Exception("Unknown output type"),
            };

            txout.Deserialize(serializationContext, mode);
            TxOut.Add(txout);
        }

        if (fUseSegwit)
        {
            TxIn.ForEach(txin =>
            {
                txin.ScriptWitness = new WitnessScript();
                txin.ScriptWitness.Deserialize(serializationContext, mode);
            });
        }
    }

    public bool IsZerocoinSpend()
    {
        if (TxIn == null) return false;
        foreach (var txin in TxIn)
            if (txin.IsZerocoinSpend()) return true;
        return false;
    }

    public bool IsBasecoin()
    {
        return !IsZerocoinSpend() && TxIn?.Count == 1 && (TxIn[0].PrevOut?.IsNull() ?? false);
    }

    public bool IsCoinStake()
    {
        if (TxIn?.Count == 0)
            return false;

        if (TxIn?.Count != 1 || !TxIn[0].IsZerocoinSpend())
            return false;

        // the coin stake transaction is marked with the first output empty
        return TxOut?.Count > 1 && TxOut[0].IsEmpty();
    }
    public bool IsZerocoinMint()
    {
        if (TxOut == null) return false;
        /*foreach (var pout in TxOut) {
            CScript script;
            if (pout->GetScriptPubKey(script))
            {
                if (script.IsZerocoinMint())
                    return true;
            }
        }*/
        return false;
    }
}

public class OutPoint
{
    public byte[]? Hash { get; set; }
    public uint N { get; set; }

    const uint ANON_MARKER = 0xffffffa0;

    public void Deserialize(VeilSerialization serializationContext, int mode)
    {
        Hash = serializationContext.ReadHash256();
        serializationContext.ReadUint(out uint n);
        N = n;
    }

    public bool IsAnonInput() => N == ANON_MARKER;
    public bool IsNull() => Hash?.SequenceEqual(VeilTransaction.ZEROHASH) ?? false;
}

public class Script
{
    public byte[]? Hash { get; set; }
    public void Deserialize(VeilSerialization serializationContext, int mode)
    {
        serializationContext.ReadCompactSize(out ulong size);
        Hash = serializationContext.ReadByteArray(size);
    }

    public static int DecodeOP_N(opcodetype opcode)
    {
        if (opcode == opcodetype.OP_0)
            return 0;
        if (!(opcode >= opcodetype.OP_1 && opcode <= opcodetype.OP_16))
            throw new Exception("DecodeOP_N");
        return (int)opcode - (int)(opcodetype.OP_1 - 1);
    }

    public int Size() => Hash?.Length ?? 0;

    public bool Empty()
    {
        if (Hash == null) return true;
        var hasValue = false;
        foreach (var b in Hash)
            if (b != 0x00)
            {
                hasValue = true;
                break;
            }
        return !hasValue;
    }

    public bool IsZerocoinMint()
    {
        if (Hash == null) return false;
        //fast test for Zerocoin Mint CScripts
        return Hash.Length > 0 && Hash[0] == (byte)opcodetype.OP_ZEROCOINMINT;
    }

    public bool IsZerocoinSpend()
    {
        if (Hash == null || Empty()) return false;

        return Hash[0] == (byte)opcodetype.OP_ZEROCOINSPEND;
    }

    public bool IsPayToScriptHash()
    {
        if (Hash == null) return false;
        // Extra-fast test for pay-to-script-hash CScripts:
        return Hash.Length == 23 &&
                Hash[0] == (byte)opcodetype.OP_HASH160 &&
                Hash[1] == 0x14 &&
                Hash[22] == (byte)opcodetype.OP_EQUAL;
    }

    public bool IsWitnessProgram(out int version, out byte[] program)
    {
        version = 0;
        program = [];
        if (Hash == null) return false;
        if (Hash.Length < 4 || Hash.Length > 42)
        {
            return false;
        }
        if (Hash[0] != (byte)opcodetype.OP_0 && (Hash[0] < (byte)opcodetype.OP_1 || Hash[0] > (byte)opcodetype.OP_16))
        {
            return false;
        }
        if ((Hash[1] + 2) == Hash.Length)
        {
            version = Converters.DecodeOP_N((opcodetype)Hash[0]);
            program = new byte[Hash.Length - 2];
            Array.Copy(Hash, 2, program, 0, program.Length);
            return true;
        }
        return false;
    }

    public bool IsPushOnly(int start)
    {
        var s = start;
        if (Hash == null) return false;
        while (s < Hash.Length)
        {
            opcodetype opcode;
            if (!GetOp(ref s, out opcode))
                return false;
            // Note that IsPushOnly() *does* consider OP_RESERVED to be a
            // push-type opcode, however execution of OP_RESERVED fails, so
            // it's not relevant to P2SH/BIP62 as the scriptSig would fail prior to
            // the P2SH special validation code being executed.
            if (opcode > opcodetype.OP_16)
                return false;
        }
        return true;
    }

    public bool GetScriptOp(ref int pc, int end, out opcodetype opcodeRet, out byte[] pvchRet)
    {

        pvchRet = [];
        opcodeRet = opcodetype.OP_INVALIDOPCODE;

        if (Hash == null) return false;

        if (pc >= end)
            return false;

        // Read instruction
        if (end - pc < 1)
            return false;
        opcodetype opcode = (opcodetype)Hash[pc++];

        // Immediate operand
        if (opcode <= opcodetype.OP_PUSHDATA4)
        {
            uint nSize = 0;
            if (opcode < opcodetype.OP_PUSHDATA1)
            {
                nSize = (uint)opcode;
            }
            else if (opcode == opcodetype.OP_PUSHDATA1)
            {
                if (end - pc < 1)
                    return false;
                nSize = Hash[pc++];
            }
            else if (opcode == opcodetype.OP_PUSHDATA2)
            {
                if (end - pc < 2)
                    return false;
                nSize = (uint)BitConverter.ToInt16(Hash, pc);

                pc += 2;
            }
            else if (opcode == opcodetype.OP_PUSHDATA4)
            {
                if (end - pc < 4)
                    return false;
                nSize = (uint)BitConverter.ToInt32(Hash, pc);
                pc += 4;
            }
            if (end - pc < 0 || (uint)(end - pc) < nSize)
                return false;
            //if (pvchRet)
            //    pvchRet->assign(pc, pc + nSize);
            pvchRet = new byte[nSize];
            Array.Copy(Hash, pc, pvchRet, 0, nSize);
            //pc += (int)nSize;
        }

        opcodeRet = opcode;
        return true;
    }


    public bool GetOp(ref int pc, ref opcodetype opcodeRet, out byte[] vchRet)
    {
        vchRet = [];
        opcodeRet = opcodetype.OP_0;
        if (Hash == null) return false;
        var pcl = pc;
        var res = GetScriptOp(ref pcl, Hash.Length, out opcodetype opr, out byte[] vch);
        if (res)
        {
            pc = pcl;
            vchRet = vch;
            opcodeRet = opr;
        }
        return res;
    }

    public bool GetOp(ref int pc, out opcodetype opcodeRet)
    {
        opcodeRet = opcodetype.OP_0;
        if (Hash == null) return false;
        var pcl = pc;
        var res = GetScriptOp(ref pcl, Hash.Length, out opcodetype tmpop, out _);
        if (res)
        {
            pc = pcl;
            opcodeRet = tmpop;
        }
        return res;
    }
}

public class WitnessScript
{
    List<byte[]>? Stack { get; set; }
    public void Deserialize(VeilSerialization serializationContext, int mode)
    {
        serializationContext.ReadCompactSize(out ulong size);

        Stack = [];

        for (var i = 0UL; i < size; i++)
        {
            serializationContext.ReadCompactSize(out ulong tempSize);
            var stackEntry = serializationContext.ReadByteArray(tempSize);
            Stack.Add(stackEntry);
        }
    }
}

public class VeilTxIn
{
    const uint SEQUENCE_LOCKTIME_MASK = 0x0000ffff;

    public long ZeroCoinSpend { get; set; }
    public OutPoint? PrevOut { get; set; }
    public Script? Script { get; set; }
    public uint NSequence { get; set; }
    public WitnessScript? ScriptData { get; set; }
    public WitnessScript? ScriptWitness { get; set; }

    public void Deserialize(VeilSerialization serializationContext, int mode)
    {
        PrevOut = new OutPoint();
        PrevOut.Deserialize(serializationContext, mode);

        Script = new Script();
        Script.Deserialize(serializationContext, mode);

        serializationContext.ReadUint(out uint nseq);
        NSequence = nseq;

        if (IsAnonInput())
        {
            ScriptData = new WitnessScript();
            ScriptData.Deserialize(serializationContext, mode);
        }

        if (IsZerocoinSpend())
            ZeroCoinSpend = (NSequence & SEQUENCE_LOCKTIME_MASK) * (long)Constants.COIN;
    }

    public bool IsAnonInput() => PrevOut?.IsAnonInput() ?? false;
    public bool IsZerocoinSpend() => Script?.IsZerocoinSpend() ?? false;
}

public class VeilTxOut
{
    public long Amount { get; set; }
    public Script? ScriptPubKey { get; set; }
    public OutputTypes OutputType { get; set; }

    public virtual void Deserialize(VeilSerialization serializationContext, int mode) { }

    public bool IsEmpty()
    {
        return Amount == 0 && (ScriptPubKey?.Empty() ?? true);
    }

    public List<string> GetAddresses()
    {
        var ret = new List<string>();
        if (ScriptPubKey == null) return ret;
        if (OutputType == OutputTypes.OUTPUT_STANDARD || OutputType == OutputTypes.OUTPUT_CT)
        {
            var addresses = new List<IDestination>();
            Converters.ExtractDestinations(ScriptPubKey, out txnouttype ctype, addresses, out int nrr);
            addresses.ForEach(addr => ret.Add(Converters.EncodeDestination(addr)));
        }
        return ret;
    }
}

public class VeilTxOutStandard : VeilTxOut
{
    public VeilTxOutStandard() => base.OutputType = OutputTypes.OUTPUT_STANDARD;

    public override void Deserialize(VeilSerialization serializationContext, int mode)
    {
        serializationContext.ReadLong(out long amount);
        Amount = amount;

        ScriptPubKey = new Script();
        ScriptPubKey.Deserialize(serializationContext, mode);
    }
}

public class VeilTxOutCT : VeilTxOut
{
    public byte[]? Commitment { get; set; }
    public byte[]? Data { get; set; }
    public byte[]? Rangeproof { get; set; }

    public VeilTxOutCT() => base.OutputType = OutputTypes.OUTPUT_CT;

    public override void Deserialize(VeilSerialization serializationContext, int mode)
    {
        Commitment = serializationContext.ReadByteArray(33);

        serializationContext.ReadCompactSize(out ulong size);
        Data = serializationContext.ReadByteArray(size);

        ScriptPubKey = new Script();
        ScriptPubKey.Deserialize(serializationContext, mode);

        serializationContext.ReadCompactSize(out size);
        Rangeproof = serializationContext.ReadByteArray(size);
    }
}

public class VeilTxOutRingCT : VeilTxOut
{
    public byte[]? PK { get; set; }
    public byte[]? Commitment { get; set; }
    public byte[]? Data { get; set; }
    public byte[]? Rangeproof { get; set; }

    public VeilTxOutRingCT() => base.OutputType = OutputTypes.OUTPUT_RINGCT;

    public override void Deserialize(VeilSerialization serializationContext, int mode)
    {
        PK = serializationContext.ReadByteArray(33);
        Commitment = serializationContext.ReadByteArray(33);

        serializationContext.ReadCompactSize(out ulong size);
        Data = serializationContext.ReadByteArray(size);

        serializationContext.ReadCompactSize(out size);
        Rangeproof = serializationContext.ReadByteArray(size);
    }
}

public class VeilTxOutData : VeilTxOut
{
    public byte[]? Data { get; set; }
    public ulong? CTFee { get; set; }

    public VeilTxOutData() => base.OutputType = OutputTypes.OUTPUT_DATA;

    public override void Deserialize(VeilSerialization serializationContext, int mode)
    {
        serializationContext.ReadCompactSize(out ulong size);
        Data = serializationContext.ReadByteArray(size);

        CTFee = null;

        if (Data.Length < 2 || Data[0] != (byte)DataOutputTypes.DO_FEE)
            return;

        var serializationSubContext = new VeilSerialization(Data);
        serializationSubContext.ReadByte(out _); // skip first byte as we used it above

        serializationSubContext.ReadVarInt(out ulong ctfeelocal);

        CTFee = ctfeelocal;
    }
}

public enum OutputTypes : byte
{
    OUTPUT_NULL = 0, // marker for CCoinsView (0.14)
    OUTPUT_STANDARD = 1,
    OUTPUT_CT = 2,
    OUTPUT_RINGCT = 3,
    OUTPUT_DATA = 4,
}

public enum opcodetype : byte
{
    // push value
    OP_0 = 0x00,
    OP_FALSE = OP_0,
    OP_PUSHDATA1 = 0x4c,
    OP_PUSHDATA2 = 0x4d,
    OP_PUSHDATA4 = 0x4e,
    OP_1NEGATE = 0x4f,
    OP_RESERVED = 0x50,
    OP_1 = 0x51,
    OP_TRUE = OP_1,
    OP_2 = 0x52,
    OP_3 = 0x53,
    OP_4 = 0x54,
    OP_5 = 0x55,
    OP_6 = 0x56,
    OP_7 = 0x57,
    OP_8 = 0x58,
    OP_9 = 0x59,
    OP_10 = 0x5a,
    OP_11 = 0x5b,
    OP_12 = 0x5c,
    OP_13 = 0x5d,
    OP_14 = 0x5e,
    OP_15 = 0x5f,
    OP_16 = 0x60,

    // control
    OP_NOP = 0x61,
    OP_VER = 0x62,
    OP_IF = 0x63,
    OP_NOTIF = 0x64,
    OP_VERIF = 0x65,
    OP_VERNOTIF = 0x66,
    OP_ELSE = 0x67,
    OP_ENDIF = 0x68,
    OP_VERIFY = 0x69,
    OP_RETURN = 0x6a,

    // stack ops
    OP_TOALTSTACK = 0x6b,
    OP_FROMALTSTACK = 0x6c,
    OP_2DROP = 0x6d,
    OP_2DUP = 0x6e,
    OP_3DUP = 0x6f,
    OP_2OVER = 0x70,
    OP_2ROT = 0x71,
    OP_2SWAP = 0x72,
    OP_IFDUP = 0x73,
    OP_DEPTH = 0x74,
    OP_DROP = 0x75,
    OP_DUP = 0x76,
    OP_NIP = 0x77,
    OP_OVER = 0x78,
    OP_PICK = 0x79,
    OP_ROLL = 0x7a,
    OP_ROT = 0x7b,
    OP_SWAP = 0x7c,
    OP_TUCK = 0x7d,

    // splice ops
    OP_CAT = 0x7e,
    OP_SUBSTR = 0x7f,
    OP_LEFT = 0x80,
    OP_RIGHT = 0x81,
    OP_SIZE = 0x82,

    // bit logic
    OP_INVERT = 0x83,
    OP_AND = 0x84,
    OP_OR = 0x85,
    OP_XOR = 0x86,
    OP_EQUAL = 0x87,
    OP_EQUALVERIFY = 0x88,
    OP_RESERVED1 = 0x89,
    OP_RESERVED2 = 0x8a,

    // numeric
    OP_1ADD = 0x8b,
    OP_1SUB = 0x8c,
    OP_2MUL = 0x8d,
    OP_2DIV = 0x8e,
    OP_NEGATE = 0x8f,
    OP_ABS = 0x90,
    OP_NOT = 0x91,
    OP_0NOTEQUAL = 0x92,

    OP_ADD = 0x93,
    OP_SUB = 0x94,
    OP_MUL = 0x95,
    OP_DIV = 0x96,
    OP_MOD = 0x97,
    OP_LSHIFT = 0x98,
    OP_RSHIFT = 0x99,

    OP_BOOLAND = 0x9a,
    OP_BOOLOR = 0x9b,
    OP_NUMEQUAL = 0x9c,
    OP_NUMEQUALVERIFY = 0x9d,
    OP_NUMNOTEQUAL = 0x9e,
    OP_LESSTHAN = 0x9f,
    OP_GREATERTHAN = 0xa0,
    OP_LESSTHANOREQUAL = 0xa1,
    OP_GREATERTHANOREQUAL = 0xa2,
    OP_MIN = 0xa3,
    OP_MAX = 0xa4,

    OP_WITHIN = 0xa5,

    // crypto
    OP_RIPEMD160 = 0xa6,
    OP_SHA1 = 0xa7,
    OP_SHA256 = 0xa8,
    OP_HASH160 = 0xa9,
    OP_HASH256 = 0xaa,
    OP_CODESEPARATOR = 0xab,
    OP_CHECKSIG = 0xac,
    OP_CHECKSIGVERIFY = 0xad,
    OP_CHECKMULTISIG = 0xae,
    OP_CHECKMULTISIGVERIFY = 0xaf,

    // expansion
    OP_NOP1 = 0xb0,
    OP_CHECKLOCKTIMEVERIFY = 0xb1,
    OP_NOP2 = OP_CHECKLOCKTIMEVERIFY,
    OP_CHECKSEQUENCEVERIFY = 0xb2,
    OP_NOP3 = OP_CHECKSEQUENCEVERIFY,
    OP_NOP4 = 0xb3,
    OP_NOP5 = 0xb4,
    OP_NOP6 = 0xb5,
    OP_NOP7 = 0xb6,
    OP_NOP8 = 0xb7,
    OP_NOP9 = 0xb8,
    OP_NOP10 = 0xb9,

    // zerocoin
    OP_ZEROCOINMINT = 0xc1,
    OP_ZEROCOINSPEND = 0xc2,

    OP_INVALIDOPCODE = 0xff,
}

public enum DataOutputTypes : byte
{
    DO_NULL = 0, // reserved
    DO_NARR_PLAIN = 1,
    DO_NARR_CRYPT = 2,
    DO_STEALTH = 3,
    DO_STEALTH_PREFIX = 4,
    DO_VOTE = 5,
    DO_FEE = 6,
    DO_DEV_FUND_CFWD = 7,
    DO_FUND_MSG = 8,
}