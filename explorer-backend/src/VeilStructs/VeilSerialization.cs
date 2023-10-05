namespace ExplorerBackend.VeilStructs;

public class VeilSerialization
{
    private byte[]? _data { get; set; }
    private int readIndex = 0;
    public VeilSerialization() { }
    public VeilSerialization(byte[] data) => _data = data;
    public static T Deserialize<T>(byte[] data, int mode) where T : new()
    {
        var serializationContext = new VeilSerialization(data);
        var objectContext = new T();

        ArgumentNullException.ThrowIfNull(objectContext);

        var serializable = objectContext as IVeilSerializable;
        serializable?.Deserialize(serializationContext, mode);

        return objectContext;
    }


    public void ReadByte(out byte b)
    {
        ArgumentNullException.ThrowIfNull(_data);

        b = _data[readIndex];
        readIndex++;
    }

    public void ReadBool(out bool bl)
    {
        ReadByte(out byte b);
        bl = b > 0;
    }

    public void ReadUint(out uint i)
    {
        ArgumentNullException.ThrowIfNull(_data);

        i = BitConverter.ToUInt32(_data, readIndex);
        readIndex += 4;
    }

    public void ReadShort(out short i)
    {
        ArgumentNullException.ThrowIfNull(_data);

        i = BitConverter.ToInt16(_data, readIndex);
        readIndex += 2;
    }

    public void ReadInt(out int i)
    {
        ArgumentNullException.ThrowIfNull(_data);

        i = BitConverter.ToInt32(_data, readIndex);
        readIndex += 4;
    }

    public void ReadLong(out long i)
    {
        ArgumentNullException.ThrowIfNull(_data);

        i = BitConverter.ToInt64(_data, readIndex);
        readIndex += 8;
    }

    public byte[] ReadHash256()
    {
        ArgumentNullException.ThrowIfNull(_data);

        var narr = new byte[32];
        Array.Copy(_data, readIndex, narr, 0, 32);
        readIndex += 32;
        return narr;
    }

    public byte[] ReadByteArray(ulong size)
    {
        ArgumentNullException.ThrowIfNull(_data);

        var narr = new byte[size];
        Array.Copy(_data, readIndex, narr, 0, (long)size);
        readIndex += (int)size; // to-do can cause bugs, readIndex should be long?
        return narr;
    }

    public void ReadVarInt(out ulong val)
    {
        // taken from NBitcoin: https://github.com/MetacoSA/NBitcoin/blob/5acf3861b33d562fece430e15be0a90e9e8dfdc9/NBitcoin/Protocol/VarInt.cs
        // modified to suit code
        var n = 0ul;
        while (true)
        {
            ReadByte(out byte chData);
            ulong a = n << 7;
            byte b = (byte)(chData & 0x7F);
            n = a | b;
            if ((chData & 0x80) != 0)
                n++;
            else
                break;
        }

        val = n;
    }

    public void ReadCompactSize(out ulong nSizeRet)
    {
        byte chSize;
        ReadByte(out chSize);

        if (chSize < 253)
        {
            nSizeRet = chSize;
        }
        else if (chSize == 253)
        {
            short sh;
            ReadShort(out sh);
            nSizeRet = (ulong)sh;
            if (nSizeRet < 253)
                throw new Exception("non-canonical ReadCompactSize()");
        }
        else if (chSize == 254)
        {
            int sh;
            ReadInt(out sh);
            nSizeRet = (ulong)sh;
            if (nSizeRet < 0x10000u)
                throw new Exception("non-canonical ReadCompactSize()");
        }
        else
        {
            long sh;
            ReadLong(out sh);
            nSizeRet = (ulong)sh;
            if (nSizeRet < 0x100000000UL)
                throw new Exception("non-canonical ReadCompactSize()");
        }
        if (nSizeRet > ulong.MaxValue)
            throw new Exception("ReadCompactSize(): size too large");
    }
}