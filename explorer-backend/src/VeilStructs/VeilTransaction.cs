namespace ExplorerBackend.VeilStructs;

public class VeilTransaction : IVeilSerializable
{
    public int Version { get; set; }
    public uint LockTime { get; set; }
    public List<VeilTxIn>? TxIn { get; set; }
    public List<VeilTxOut>? TxOut { get; set; }

    public void Deserialize(VeilSerialization serializationContext, int mode)
    {
        byte bv;
        serializationContext.ReadByte(out bv);

        Version = bv;

        serializationContext.ReadByte(out bv);
        Version |= bv << 8;

        var fUseSegwit = false;
        serializationContext.ReadBool(out fUseSegwit);

        var lockTime = 0u;
        serializationContext.ReadUint(out lockTime);
        LockTime = lockTime;

        var vInCount = 0UL;
        serializationContext.ReadCompactSize(out vInCount);

        TxIn = new List<VeilTxIn>();
        for (var i = 0UL; i < vInCount; i++)
        {
            var txin = new VeilTxIn();
            txin.Deserialize(serializationContext, mode);
            TxIn.Add(txin);
        }


        var vOutCount = 0UL;
        serializationContext.ReadCompactSize(out vOutCount);

        TxOut = new List<VeilTxOut>();
        for (var i = 0UL; i < vOutCount; i++)
        {
            byte outputType = 0;
            serializationContext.ReadByte(out outputType);

            VeilTxOut txout;

            switch ((OutputTypes)outputType)
            {
                case OutputTypes.OUTPUT_STANDARD:
                    txout = new VeilTxOutStandard();
                    break;
                case OutputTypes.OUTPUT_CT:
                    txout = new VeilTxOutCT();
                    break;
                case OutputTypes.OUTPUT_RINGCT:
                    txout = new VeilTxOutRingCT();
                    break;
                case OutputTypes.OUTPUT_DATA:
                    txout = new VeilTxOutData();
                    break;
                default:
                    throw new Exception("Unknown output type");
            }
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
}

public class OutPoint
{
    public byte[]? Hash { get; set; }
    public uint N { get; set; }

    const uint ANON_MARKER = 0xffffffa0;

    public void Deserialize(VeilSerialization serializationContext, int mode)
    {
        Hash = serializationContext.ReadHash256();
        var n = 0u;
        serializationContext.ReadUint(out n);
        N = n;
    }

    public bool IsAnonInput() => N == ANON_MARKER;
}

public class Script
{
    public byte[]? Hash { get; set; }
    public void Deserialize(VeilSerialization serializationContext, int mode)
    {
        var size = 0UL;
        serializationContext.ReadCompactSize(out size);
        Hash = serializationContext.ReadByteArray(size);
    }
}

public class WitnessScript
{
    List<byte[]>? Stack { get; set; }
    public void Deserialize(VeilSerialization serializationContext, int mode)
    {
        var size = 0UL;
        var tempSize = 0UL;
        serializationContext.ReadCompactSize(out size);

        Stack = new List<byte[]>();

        for (var i = 0UL; i < size; i++)
        {
            serializationContext.ReadCompactSize(out tempSize);
            var stackEntry = serializationContext.ReadByteArray(tempSize);
            Stack.Add(stackEntry);
        }
    }
}

public class VeilTxIn
{
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

        var nseq = 0u;
        serializationContext.ReadUint(out nseq);
        NSequence = nseq;

        if (IsAnonInput())
        {
            ScriptData = new WitnessScript();
            ScriptData.Deserialize(serializationContext, mode);
        }
    }

    public bool IsAnonInput() => PrevOut?.IsAnonInput() ?? false;
}

public class VeilTxOut
{
    public long Amount { get; set; }
    public Script? ScriptPubKey { get; set; }
    public OutputTypes OutputType { get; set; }

    public virtual void Deserialize(VeilSerialization serializationContext, int mode) { }
}

public class VeilTxOutStandard : VeilTxOut
{
    public VeilTxOutStandard() => base.OutputType = OutputTypes.OUTPUT_STANDARD;

    public override void Deserialize(VeilSerialization serializationContext, int mode)
    {
        var amount = 0L;
        serializationContext.ReadLong(out amount);
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

        var size = 0UL;
        serializationContext.ReadCompactSize(out size);
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

        var size = 0UL;
        serializationContext.ReadCompactSize(out size);
        Data = serializationContext.ReadByteArray(size);

        serializationContext.ReadCompactSize(out size);
        Rangeproof = serializationContext.ReadByteArray(size);
    }
}

public class VeilTxOutData : VeilTxOut
{
    public byte[]? Data { get; set; }

    public VeilTxOutData() => base.OutputType = OutputTypes.OUTPUT_DATA;

    public override void Deserialize(VeilSerialization serializationContext, int mode)
    {
        var size = 0UL;
        serializationContext.ReadCompactSize(out size);
        Data = serializationContext.ReadByteArray(size);
    }
}

public enum OutputTypes : byte
{
    OUTPUT_NULL = 0, // marker for CCoinsView (0.14)
    OUTPUT_STANDARD = 1,
    OUTPUT_CT = 2,
    OUTPUT_RINGCT = 3,
    OUTPUT_DATA = 4,
};