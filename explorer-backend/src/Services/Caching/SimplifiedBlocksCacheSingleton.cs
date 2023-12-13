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
    private readonly ILogger _logger;
    private readonly IOptionsMonitor<ExplorerConfig> _explorerConfig;
    
    public SimplifiedBlocksCacheSingleton(IOptionsMonitor<ExplorerConfig> explorerConfig, ILogger logger)
    {
        _explorerConfig = explorerConfig;
        _logger = logger;

        _oldestBlocksBufferCapacity = _explorerConfig.CurrentValue.OldestSimplifiedBlocksCacheCount;
        _blocksBufferCapacity = _explorerConfig.CurrentValue.SimplifiedBlocksCacheCount;

        _oldestBlocksBuffer = GC.AllocateUninitializedArray<byte>(_oldestBlocksBufferCapacity * _BytesInBlock, pinned: true);
        _blocksBuffer = GC.AllocateUninitializedArray<byte>(_blocksBufferCapacity * _BytesInBlock, pinned: true);
    }

    public bool IsInCacheRange(int value) => _latestBlockHeight -_blocksBufferCapacity <= value && value < _latestBlockHeight;
    // writing blocks
    public async Task SetBlockCache(GetBlockResult block, bool isLatestBlock = false)
    {
        if(block is null || block.Height < 1)
        {
            _logger.LogInformation("can't set simplifiedBLock cache");
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
            int differenceOfHeight = block.Height -_latestBlockHeight;
            if(differenceOfHeight < 1) goto PrevBlock;

            _latestBlockHeight = block.Height;
            long offset;

            if(differenceOfHeight > 1)
            {
                if(_latestBlockPosition + differenceOfHeight - 1 > _blocksBufferCapacity - 1)
                    offset = (_latestBlockPosition + differenceOfHeight - 1 - _blocksBufferCapacity) * _BytesInBlock;
                else
                    offset = (_latestBlockPosition + differenceOfHeight - 1) * _BytesInBlock;
            }
            else    
                offset = _latestBlockPosition * _BytesInBlock;

            SerializeBlock(block, _blocksBuffer, offset);

            _latestBlockPosition += differenceOfHeight;

            if(_latestBlockPosition > _blocksBufferCapacity - 1)            
                _latestBlockPosition -= _blocksBufferCapacity;            

           return;
        }

        PrevBlock:
        if(block.Height < _latestBlockHeight && block.Height > (_latestBlockHeight - _blocksBufferCapacity))
        {
            _semaphoreSlim.Wait(1000);

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
}
