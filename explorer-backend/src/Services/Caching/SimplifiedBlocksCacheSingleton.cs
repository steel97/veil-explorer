using Microsoft.Extensions.Options;
using ExplorerBackend.Configs;
using ExplorerBackend.Models.API;
using ExplorerBackend.Models.Data;
using ExplorerBackend.Models.Node.Response;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Diagnostics;

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
    private readonly IBlocksService _blocksService;
    private readonly ILogger<SimplifiedBlocksCacheSingleton> _logger;
    private readonly IOptionsMonitor<MemoryCacheConfig> _memoryCacheConfig;

    public SimplifiedBlocksCacheSingleton(IOptionsMonitor<MemoryCacheConfig> memoryCacheConfig, BlocksService blocksService, ILogger<SimplifiedBlocksCacheSingleton> logger)
    {
        _memoryCacheConfig = memoryCacheConfig;
        _logger = logger;
        _blocksService = blocksService;

        ArgumentNullException.ThrowIfNull(_memoryCacheConfig.CurrentValue.OldestSimplifiedBlocksCacheCount);
        ArgumentNullException.ThrowIfNull(_memoryCacheConfig.CurrentValue.SimplifiedBlocksCacheCount);

        _oldestBlocksBufferCapacity = _memoryCacheConfig.CurrentValue.OldestSimplifiedBlocksCacheCount;
        _blocksBufferCapacity = _memoryCacheConfig.CurrentValue.SimplifiedBlocksCacheCount;

        _oldestBlocksBuffer = GC.AllocateUninitializedArray<byte>(_oldestBlocksBufferCapacity * _BytesInBlock, pinned: true);
        _blocksBuffer = GC.AllocateUninitializedArray<byte>(_blocksBufferCapacity * _BytesInBlock, pinned: true);
    }

    public bool IsInCacheRange(int height) =>
        (height >= 1 && height <= _oldestBlocksBufferCapacity) ||
        (height <= _latestBlockHeight && height > _latestBlockHeight - _blocksBufferCapacity);

    // writing blocks
    public void SetBlockCache(GetBlockResult block, bool isLatestBlock = false)
    {
        if (block is null || block.Height < 1)
        {
            _logger.LogInformation("can't set {service} cache. probably cuz block data was null", nameof(SimplifiedBlocksCacheSingleton));
            return;
        }

        if (block.Height <= _oldestBlocksBufferCapacity)
        {
            long offset = (block.Height * _BytesInBlock) - _BytesInBlock;

            SerializeBlock(block, _oldestBlocksBuffer, (nuint)offset);
            return;
        }

        _semaphoreSlim.Wait(10);

        if (isLatestBlock)
        {
            int differenceOfHeight = block.Height - _latestBlockHeight;
            if (differenceOfHeight < 1) goto PrevBlock;

            _latestBlockHeight = block.Height;
            long offset;

            if (differenceOfHeight > 1)
            {
                if (_latestBlockPosition + differenceOfHeight - 1 > _blocksBufferCapacity - 1)
                {
                    while (differenceOfHeight > _blocksBufferCapacity)
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

            SerializeBlock(block, _blocksBuffer, (nuint)offset);

            _latestBlockPosition += differenceOfHeight;

            while (_latestBlockPosition > _blocksBufferCapacity - 1)
                _latestBlockPosition -= _blocksBufferCapacity;

            _semaphoreSlim.Release();
            return;
        }

        PrevBlock:
        if (block.Height < _latestBlockHeight && block.Height > (_latestBlockHeight - _blocksBufferCapacity))
        {
            int positionToWrite = _latestBlockPosition - (_latestBlockHeight - block.Height) - 1;

            while (positionToWrite > _blocksBufferCapacity)
                positionToWrite -= _blocksBufferCapacity;

            long offset = positionToWrite * _BytesInBlock;

            SerializeBlock(block, _blocksBuffer, (nuint)offset);

            _semaphoreSlim.Release();
            return;
        }

        _semaphoreSlim.Release();
        return;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SerializeBlock(GetBlockResult block, byte[] buffer, nuint offset)
    {
        ref byte bufferRef = ref MemoryMarshal.GetArrayDataReference(buffer);
        BlockType blockType = _blocksService.GetBlockType(block.Proof_type!);

        Unsafe.WriteUnaligned(ref Unsafe.Add(ref bufferRef, offset), block.Size);
        offset += sizeof(int);
        Unsafe.WriteUnaligned(ref Unsafe.Add(ref bufferRef, offset), block.Weight);
        offset += sizeof(int);
        Unsafe.WriteUnaligned(ref Unsafe.Add(ref bufferRef, offset), (byte)blockType);
        offset += sizeof(byte);
        Unsafe.WriteUnaligned(ref Unsafe.Add(ref bufferRef, offset), block.Time);
        offset += sizeof(long);
        Unsafe.WriteUnaligned(ref Unsafe.Add(ref bufferRef, offset), block.Mediantime);
        offset += sizeof(long);
        Unsafe.WriteUnaligned(ref Unsafe.Add(ref bufferRef, offset), (ushort)block.NTx);
    }

    // reading blocks
    public bool GetSimplifiedBlock(SimplifiedBlock block, int height)
    {
        long offset;
        if (height >= 1 && height <= _oldestBlocksBufferCapacity)
        {
            offset = (height * _BytesInBlock) - _BytesInBlock;
            return DeserializeBlock(_oldestBlocksBuffer, (nuint)offset, block);            
        }

        if (height == _latestBlockHeight)
        {
            offset = _latestBlockPosition * _BytesInBlock - _BytesInBlock;
            if (offset < 0)
                offset = 0;

            return DeserializeBlock(_blocksBuffer, (nuint)offset, block);
        }

        if (height < _latestBlockHeight && height > (_latestBlockHeight - _blocksBufferCapacity))
        {
            int blockIndex = _latestBlockPosition - (_latestBlockHeight - height) - 1;

            if (blockIndex < 0)
                blockIndex = _blocksBufferCapacity + blockIndex;

            offset = blockIndex * _BytesInBlock;

            return DeserializeBlock(_blocksBuffer, (nuint)offset, block);
        }

        return false;
    }

    public void GetSimplifiedBlocksRange(List<SimplifiedBlock> blocksList, int height, int count, SortDirection sort, out List<uint>? missedCacheBlocksList)
    {
        missedCacheBlocksList = null;

        _semaphoreSlim.Wait(10);

        int blockPosition = _latestBlockPosition - (_latestBlockHeight - height);

        if (blockPosition - count >= 0 && height <= _latestBlockHeight && height >= _latestBlockHeight - 15 && count <= 15)
        {
            int arrayLength = count * _BytesInBlock;
            int bufferOffset;

            if (blockPosition < 0)
                blockPosition += _blocksBufferCapacity;

            bufferOffset = blockPosition * _BytesInBlock;

            Span<byte> buffer = stackalloc byte[arrayLength];            
            _blocksBuffer.AsSpan(bufferOffset - arrayLength, arrayLength).CopyTo(buffer);

            _semaphoreSlim.Release();

            int offsetSpan = sort is SortDirection.DESC ? arrayLength - _BytesInBlock : 0;
            foreach (var block in blocksList)
            {
                bool deserializationSuccess = DeserializeBlock(buffer.Slice(offsetSpan, _BytesInBlock), block);
                if(!deserializationSuccess)
                {
                    missedCacheBlocksList ??= new(count);
                    missedCacheBlocksList.Add((uint)block.Height);
                }
                offsetSpan = Advance(offsetSpan, sort);
            }
            return;
        }

        foreach (var block in blocksList)
        {
            bool deserializationSuccess = GetSimplifiedBlock(block, block.Height);
            if(!deserializationSuccess)
            {
                missedCacheBlocksList ??= new(count);
                missedCacheBlocksList.Add((uint)block.Height);
            } 
        }
        _semaphoreSlim.Release();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool DeserializeBlock(byte[] buffer, nuint offset, SimplifiedBlock block)
    {
        ref byte bufferRef = ref MemoryMarshal.GetArrayDataReference(buffer);

        block.Size = Unsafe.ReadUnaligned<int>(ref Unsafe.Add(ref bufferRef, offset));

        if (block.Size <= 0) return false;

        offset += sizeof(int);
        block.Weight = Unsafe.ReadUnaligned<int>(ref Unsafe.Add(ref bufferRef, offset));
        offset += sizeof(int);
        block.ProofType = Unsafe.ReadUnaligned<BlockType>(ref Unsafe.Add(ref bufferRef, offset));
        offset += sizeof(byte);
        block.Time = Unsafe.ReadUnaligned<long>(ref Unsafe.Add(ref bufferRef, offset));
        offset += sizeof(long);
        block.MedianTime = Unsafe.ReadUnaligned<long>(ref Unsafe.Add(ref bufferRef, offset));
        offset += sizeof(long);
        block.TxCount = Unsafe.ReadUnaligned<ushort>(ref Unsafe.Add(ref bufferRef, offset));

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool DeserializeBlock(ReadOnlySpan<byte> buffer, SimplifiedBlock block)
    {
        int offsetSpan = 0;

        ReadOnlySpan<byte> spanBufferSliced = buffer.Slice(offsetSpan, sizeof(int));

        block.Size = MemoryMarshal.Read<int>(spanBufferSliced);

        if (block.Size <= 0) return false;

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

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int Advance(int value, SortDirection sort) => sort switch
    {
        SortDirection.DESC => value - _BytesInBlock,
        _ => value + _BytesInBlock
    };
}
