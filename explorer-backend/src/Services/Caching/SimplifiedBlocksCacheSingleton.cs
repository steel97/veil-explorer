using Microsoft.Extensions.Options;
using ExplorerBackend.Configs;
using ExplorerBackend.Models.API;
using ExplorerBackend.Models.Data;
using ExplorerBackend.Models.Node.Response;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace ExplorerBackend.Services.Caching;

public class SimplifiedBlocksCacheSingleton
{   
    private readonly byte[] _oldestBlocksBuffer; // oldest 20 010 blocks
    private readonly byte[] _blocksBuffer; // latest 200 010 blocks
    private readonly int _oldestBlocksBufferCapacity;
    private readonly int _blocksBufferCapacity;
    private static int _latestBlockHeight = default;
    private static int _latestBlockPosition = default;
    private const byte _BytesInBlock = 27; // SimplifiedBlock without height
    private static readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
    private readonly ILogger<SimplifiedBlocksCacheSingleton> _logger;
    private readonly IOptionsMonitor<MemoryCacheConfig> _memoryCacheConfig;
    
    public SimplifiedBlocksCacheSingleton(IOptionsMonitor<MemoryCacheConfig> memoryCacheConfig, ILogger<SimplifiedBlocksCacheSingleton> logger)
    {
        _memoryCacheConfig = memoryCacheConfig;
        _logger = logger;

        ArgumentNullException.ThrowIfNull(_memoryCacheConfig.CurrentValue.OldestSimplifiedBlocksCacheCount);
        ArgumentNullException.ThrowIfNull(_memoryCacheConfig.CurrentValue.SimplifiedBlocksCacheCount);

        _oldestBlocksBufferCapacity = _memoryCacheConfig.CurrentValue.OldestSimplifiedBlocksCacheCount;
        _blocksBufferCapacity = _memoryCacheConfig.CurrentValue.SimplifiedBlocksCacheCount;

        _oldestBlocksBuffer = GC.AllocateUninitializedArray<byte>(_oldestBlocksBufferCapacity * _BytesInBlock, pinned: true);
        _blocksBuffer = GC.AllocateUninitializedArray<byte>(_blocksBufferCapacity * _BytesInBlock, pinned: true);
    }

    public bool IsInCacheRange(int value)
    {
        if(value >= 1 && value <= _oldestBlocksBufferCapacity)
            return true;

        if(value <= _latestBlockHeight && value > _latestBlockHeight - _blocksBufferCapacity)
            return true;

        return false;
    }

    // writing blocks
    public void SetBlockCache(GetBlockResult block, bool isLatestBlock = false)
    {
        if(block is null || block.Height < 1)
        {
            _logger.LogInformation("can't set {service} cache. probably cuz block data was null", nameof(SimplifiedBlocksCacheSingleton));
            return;
        }

        if(block.Height <= _oldestBlocksBufferCapacity)
        {
            long offset = (block.Height * _BytesInBlock) - _BytesInBlock;

            SerializeBlock(block, _oldestBlocksBuffer, offset);
            return;
        }

        if(isLatestBlock)
        {
            int differenceOfHeight = block.Height - _latestBlockHeight;
            if(differenceOfHeight < 1) goto PrevBlock;

            _latestBlockHeight = block.Height;
            long offset;

            if(differenceOfHeight > 1)
            {
                if(_latestBlockPosition + differenceOfHeight - 1 > _blocksBufferCapacity - 1)
                {
                    while(differenceOfHeight > _blocksBufferCapacity)
                    {
                        differenceOfHeight -= _blocksBufferCapacity;
                    }
                    offset = (_latestBlockPosition + differenceOfHeight - 1) * _BytesInBlock;
                }
                else
                    offset = (_latestBlockPosition + differenceOfHeight - 1) * _BytesInBlock;
            }
            else    
                offset = _latestBlockPosition * _BytesInBlock;

            SerializeBlock(block, _blocksBuffer, offset);

            _latestBlockPosition += differenceOfHeight;

            while(_latestBlockPosition > _blocksBufferCapacity - 1)
                _latestBlockPosition -= _blocksBufferCapacity;

           return;
        }

        PrevBlock:
        if(block.Height < _latestBlockHeight && block.Height > (_latestBlockHeight - _blocksBufferCapacity))
        {
            _semaphoreSlim.Wait(10);

            int positionToWrite = _latestBlockPosition - (_latestBlockHeight - block.Height) - 1;

            if(positionToWrite < _blocksBufferCapacity)
                positionToWrite += _blocksBufferCapacity;
            
            long offset = positionToWrite * _BytesInBlock;

            SerializeBlock(block, _blocksBuffer, offset);

            _semaphoreSlim.Release();
        }

        return;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SerializeBlock(GetBlockResult block, byte[] buffer, long offset)
    {
        ref byte bufferRef = ref MemoryMarshal.GetArrayDataReference(buffer);
        BlockType blockType = block.Proof_type switch
        {
            "Proof-of-Work (X16RT)" => BlockType.POW_X16RT,
            "Proof-of-work (ProgPow)" => BlockType.POW_ProgPow,
            "Proof-of-work (RandomX)" => BlockType.POW_RandomX,
            "Proof-of-work (Sha256D)" => BlockType.POW_Sha256D,
            "Proof-of-Stake" => BlockType.POS,
            _ => BlockType.UNKNOWN
        };

        Unsafe.WriteUnaligned(ref Unsafe.Add(ref bufferRef, (nuint)offset), block.Size);
        offset += sizeof(int);
        Unsafe.WriteUnaligned(ref Unsafe.Add(ref bufferRef, (nuint)offset), block.Weight);
        offset += sizeof(int);
        Unsafe.WriteUnaligned(ref Unsafe.Add(ref bufferRef, (nuint)offset), (byte)blockType);
        offset += sizeof(byte);
        Unsafe.WriteUnaligned(ref Unsafe.Add(ref bufferRef, (nuint)offset), block.Time);
        offset += sizeof(long);
        Unsafe.WriteUnaligned(ref Unsafe.Add(ref bufferRef, (nuint)offset), block.Mediantime);
        offset += sizeof(long);
        Unsafe.WriteUnaligned(ref Unsafe.Add(ref bufferRef, (nuint)offset), (ushort)block.NTx);
    }
    
    // reading blocks
    public SimplifiedBlock? GetSimplifiedBlock(int height)
    {
        if(height >= 1 && height <= _oldestBlocksBufferCapacity)
        {
            long offset = (height * _BytesInBlock) - _BytesInBlock;
            return DeserializeBlock(_oldestBlocksBuffer, offset, height);
        }

        if(height == _latestBlockHeight)
        {
            long offset = _latestBlockPosition * _BytesInBlock - _BytesInBlock;
            if(offset < 0)
                offset = 0;
            return DeserializeBlock(_blocksBuffer, offset, height);
        }

        if(height < _latestBlockHeight && height > (_latestBlockHeight - _blocksBufferCapacity))
        {
            int blockIndex = _latestBlockPosition - (_latestBlockHeight - height) - 1;
            
            long offset;
            if( blockIndex < 0)
            {
                blockIndex = _blocksBufferCapacity + blockIndex;
                offset = blockIndex * _BytesInBlock;
            }
            else
            {
                offset = blockIndex * _BytesInBlock;
            }
            if(offset < 0)
                offset = 0;

            return DeserializeBlock(_blocksBuffer, offset, height);
        }

        return null;
    }

    public void GetSimplifiedBlocksRange(int height, int count, List<SimplifiedBlock> blocksList, out List<byte> notCachedBlockOffset)
    {
        notCachedBlockOffset = [];
        byte offsetDiff = 0;

        if (_latestBlockPosition + count < _blocksBufferCapacity && height <= _latestBlockHeight && height >= _latestBlockHeight - 15 && count <= 15)
        {
            int arrayLenght = count * _BytesInBlock;
            int blockPosition = _latestBlockPosition - (_latestBlockHeight - height);

            Span<byte> bytes = stackalloc byte[arrayLenght];
            _blocksBuffer.AsSpan(blockPosition, arrayLenght).CopyTo(bytes);

            for (int offset = 0; offset < arrayLenght; offset += _BytesInBlock, offsetDiff++)
            {
                SimplifiedBlock? block = DeserializeBlock(height, bytes, offset);

                if(block is null) notCachedBlockOffset.Add(offsetDiff);                
                else blocksList.Add(block);
            }
            return;
        }

        for (int i = height; offsetDiff < (byte)count; i--, offsetDiff++)
        {
            SimplifiedBlock? block = GetSimplifiedBlock(i);

            if(block is null) notCachedBlockOffset.Add(offsetDiff);            
            else blocksList.Add(block); 
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static SimplifiedBlock? DeserializeBlock(byte[] buffer, long offset, int height)
    {
        ref byte bufferRef = ref MemoryMarshal.GetArrayDataReference(buffer);

        int size = Unsafe.ReadUnaligned<int>(ref Unsafe.Add(ref bufferRef, (nuint)offset));
        if(size <= 0) return default;

        SimplifiedBlock block = new()
        {
            Height = height,
            Size = size
        };

        offset += sizeof(int);
        block.Weight = Unsafe.ReadUnaligned<int>(ref Unsafe.Add(ref bufferRef, (nuint)offset));
        offset += sizeof(int);
        block.ProofType = Unsafe.ReadUnaligned<BlockType>(ref Unsafe.Add(ref bufferRef, (nuint)offset));
        offset += sizeof(byte);
        block.Time = Unsafe.ReadUnaligned<long>(ref Unsafe.Add(ref bufferRef, (nuint)offset));
        offset += sizeof(long);
        block.MedianTime = Unsafe.ReadUnaligned<long>(ref Unsafe.Add(ref bufferRef, (nuint)offset));
        offset += sizeof(long);
        block.TxCount = Unsafe.ReadUnaligned<ushort>(ref Unsafe.Add(ref bufferRef, (nuint)offset));

        return block;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static SimplifiedBlock? DeserializeBlock(int height, ReadOnlySpan<byte> buffer, int offsetSpan)
    {
        int size = MemoryMarshal.Read<int>(buffer);
        ReadOnlySpan<byte> spanBufferSliced = buffer.Slice(offsetSpan, sizeof(int));

        if(size <= 0) return default;

        SimplifiedBlock block = new()
        {
            Height = height,
            Size = size
        };
        offsetSpan += sizeof(int);
        spanBufferSliced = buffer.Slice(offsetSpan, sizeof(int));
        block.Weight = MemoryMarshal.Read<int>(spanBufferSliced);

        offsetSpan += sizeof(int);
        spanBufferSliced = buffer.Slice(offsetSpan, sizeof(byte));
        block.ProofType = MemoryMarshal.Read<BlockType>(spanBufferSliced);

        offsetSpan += sizeof(byte);
        spanBufferSliced = buffer.Slice(offsetSpan, sizeof(long));
        block.Time = MemoryMarshal.Read<long>(spanBufferSliced);

        offsetSpan += sizeof(long);
        spanBufferSliced = buffer.Slice(offsetSpan, sizeof(long));
        block.MedianTime = MemoryMarshal.Read<long>(spanBufferSliced);

        offsetSpan += sizeof(long);
        spanBufferSliced = buffer.Slice(offsetSpan, sizeof(ushort));
        block.TxCount = MemoryMarshal.Read<ushort>(spanBufferSliced);

        return block;
    }
}
