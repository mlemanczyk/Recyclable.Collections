﻿using Recyclable.Collections.Pools;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Recyclable.Collections
{
	public partial class RecyclableLongList<T>
	{
		internal static class Helpers
		{
			[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
			public static void CopyFollowingItems(RecyclableLongList<T> list, long destinationItemIndex)
			{
				int blockSizeMinus1 = list._blockSizeMinus1,
					targetItemIndex = (int)(destinationItemIndex & blockSizeMinus1),
					targetBlockIndex = (int)(destinationItemIndex >> list._blockSizePow2BitShift) + (targetItemIndex + 1 == 0 ? 1 : 0),
					lastTakenBlockIndex = list._lastBlockWithData - 1;

				// Each iteration should
				// 1. copy the remaining items in the current block (those after the deleted item)
				// 2. copy the 1st item from the next block. We deleted 1 item which leaves 1 item gap in the block.
				//    We need to fill it.

				T[][] memoryBlocks = list._memoryBlocks;
				T[] currentBlock = memoryBlocks[targetBlockIndex];
				new ReadOnlySpan<T>(currentBlock, targetItemIndex + 1, blockSizeMinus1 - targetItemIndex)
					.CopyTo(new Span<T>(currentBlock, targetItemIndex, blockSizeMinus1 - targetItemIndex));
					
				while (targetBlockIndex < lastTakenBlockIndex)
				{
					new Span<T>(currentBlock)[blockSizeMinus1] = (currentBlock = memoryBlocks[++targetBlockIndex])[0];
					new ReadOnlySpan<T>(currentBlock, 1, blockSizeMinus1)
						.CopyTo(currentBlock);
				}

				if (targetBlockIndex == lastTakenBlockIndex)
				{
					new Span<T>(currentBlock)[blockSizeMinus1] = (currentBlock = memoryBlocks[targetBlockIndex + 1])[0];
					new ReadOnlySpan<T>(currentBlock, 1, list._nextItemIndex != 0 ? list._nextItemIndex - 1 : blockSizeMinus1)
						.CopyTo(currentBlock);
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
			public static void MakeRoomAndSet(RecyclableLongList<T> list, long startingItemIndex, T item)
			{
				T[] currentBlock;
				T[][] memoryBlocks = list._memoryBlocks;
				int blockSizeMinus1 = list._blockSizeMinus1,
					targetItemIndex = (int)(startingItemIndex & blockSizeMinus1),
					targetBlockIndex = (int)(startingItemIndex >> list._blockSizePow2BitShift) + (targetItemIndex + 1 == 0 ? 1 : 0),
					lastTakenBlockIndex = list._lastBlockWithData;

				if (list._nextItemIndex == 0)
				{
					new Span<T>(memoryBlocks[lastTakenBlockIndex + 1])[0] = memoryBlocks[lastTakenBlockIndex][blockSizeMinus1];
				}

				int currentBlockIndex = lastTakenBlockIndex;
				while (currentBlockIndex > targetBlockIndex)
				{
					new ReadOnlySpan<T>(currentBlock = memoryBlocks[currentBlockIndex--], 0, blockSizeMinus1)
						.CopyTo(new Span<T>(currentBlock, 1, blockSizeMinus1));						
					new Span<T>(currentBlock)[0] = memoryBlocks[currentBlockIndex][blockSizeMinus1];
				}

				new ReadOnlySpan<T>(currentBlock = memoryBlocks[targetBlockIndex], targetItemIndex, blockSizeMinus1 - targetItemIndex)
					.CopyTo(new Span<T>(currentBlock, targetItemIndex + 1, blockSizeMinus1 - targetItemIndex));

				new Span<T>(memoryBlocks[targetBlockIndex])[targetItemIndex] = item;
			}

			public static void CopyTo(T[][] sourceMemoryBlocks, long startingIndex, int blockSize, long itemsCount, T[] destinationArray, int destinationArrayIndex)
			{
				if (itemsCount <= 0)
				{
					return;
				}

                                ReadOnlySpan<T> sourceBlockMemory;
                                Span<T> destinationArrayMemory;
                                int blockSizePow2Shift = BitOperations.TrailingZeroCount((uint)blockSize);
                                int startingBlockIndex = (int)(startingIndex >> blockSizePow2Shift);
                                int lastBlockIndex = (int)((itemsCount >> blockSizePow2Shift) + ((itemsCount & (blockSize - 1)) != 0 ? 1 : 0)) - 1;
				ReadOnlySpan<T[]> sourceMemoryBlocksSpan = new(sourceMemoryBlocks, startingBlockIndex, lastBlockIndex + 1);
				destinationArrayMemory = new Span<T>(destinationArray, destinationArrayIndex, (int)Math.Min(destinationArray.Length - destinationArrayIndex, itemsCount));
				int memoryBlockIndex;
				for (memoryBlockIndex = 0; memoryBlockIndex < lastBlockIndex; memoryBlockIndex++)
				{
					sourceBlockMemory = new(sourceMemoryBlocksSpan[memoryBlockIndex], 0, blockSize);
					sourceBlockMemory.CopyTo(destinationArrayMemory);
					destinationArrayMemory = destinationArrayMemory[blockSize..];
				}

				if (itemsCount - (lastBlockIndex * blockSize) != 0)
				{
					sourceBlockMemory = new(sourceMemoryBlocksSpan[lastBlockIndex], 0, (int)(itemsCount - (lastBlockIndex * blockSize)));
					sourceBlockMemory.CopyTo(destinationArrayMemory);
				}
			}

			public static void CopyTo(T[][] sourceMemoryBlocks, long startingIndex, int blockSize, long itemsCount, Array destinationArray, int destinationArrayIndex)
			{
				if (itemsCount <= 0)
				{
					return;
				}

                                int blockSizePow2Shift = BitOperations.TrailingZeroCount((uint)blockSize);
                                int startingBlockIndex = (int)(startingIndex >> blockSizePow2Shift);
                                int lastBlockIndex = (int)((itemsCount >> blockSizePow2Shift) + ((itemsCount & (blockSize - 1)) != 0 ? 1 : 0)) - 1;

				var destinationIndex = destinationArrayIndex;
				ReadOnlySpan<T[]> sourceMemoryBlocksSpan = new(sourceMemoryBlocks, startingBlockIndex, lastBlockIndex + 1);
				for (int memoryBlockIndex = 0; memoryBlockIndex < lastBlockIndex; memoryBlockIndex++)
				{
					Array.ConstrainedCopy(sourceMemoryBlocksSpan[memoryBlockIndex], 0, destinationArray, destinationIndex, blockSize);
					destinationIndex += blockSize;
				}

				if (itemsCount - destinationIndex > 0)
				{
					Array.ConstrainedCopy(sourceMemoryBlocksSpan[lastBlockIndex], 0, destinationArray, destinationIndex, (int)(itemsCount - destinationIndex));
				}
			}

			/// <summary>
			/// Creates a set of new memory buffers, if needed, to allow storing at minimum <paramref name="newCapacity"/> no. of items.
			/// </summary>
			/// <param name="list"><see cref="RecyclableLongList{T}"/> that needs to be resized.</param>
			/// <param name="minBlockSize">Minimal requested block size. It MUST be rounded to the power of 2, see remarks.</param>
			/// <param name="minBlockSizePow2Shift">Pre-calculated bit shifting value for left & right shift operations against<paramref name="minBlockSize"/>.</param>
			/// <param name="newCapacity">The minimum no. of items <paramref name="list"/> MUST be able to store after <see cref="RecyclableLongList{T}.Resize(RecyclableLongList{T}, int, byte, long)"/>.</param>
			/// <remarks><para>
			/// For performance reasons, <paramref name="minBlockSize"/> MUST a power of 2. This simplifies a lot block & item
			/// index calculations, i.e. makes them logical operations on bits.
			/// </para>
			/// <para>This method checks for integral overflow.</para>
			/// </remarks>
			/// <remarks>
			/// This method doesn't support downsizing the memory block. As such it doesn't release excessive blocks. This is to for additional performance
			/// gain - 1+ operation less for each call. If you need this functionality, you need to implement it on a higher level.
			/// </remarks>
			/// <returns>The maximum no. of items <paramref name="list"/> can store.</returns>
			[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
			public static long Resize(RecyclableLongList<T> list, int minBlockSize, byte minBlockSizePow2Shift, long newCapacity)
			{
				int sourceBlockCount = list._memoryBlocks?.Length ?? 0;
				int requiredBlockCount = checked((int)(newCapacity >> minBlockSizePow2Shift) + ((newCapacity & (minBlockSize - 1)) != 0 ? 1 : 0));
				int blockIndex;

				T[][] newMemoryBlocks;
				if (requiredBlockCount > sourceBlockCount)
				{
					// Allocate new memory block for all arrays
					newMemoryBlocks = requiredBlockCount >= RecyclableDefaults.MinPooledArrayLength ? RecyclableArrayPool<T[]>.RentShared(requiredBlockCount) : new T[requiredBlockCount][];

					// Copy arrays from the old memory block for all arrays
					if (sourceBlockCount > 0)
					{
						Array.Copy(list._memoryBlocks!, newMemoryBlocks, sourceBlockCount);
						// We can now return the old memory block for all arrays itself
						if (sourceBlockCount >= RecyclableDefaults.MinPooledArrayLength)
						{
							RecyclableArrayPool<T[]>.ReturnShared(list._memoryBlocks!, true);
						}
					}

					list._memoryBlocks = newMemoryBlocks;
				}
				else
				{
					newMemoryBlocks = list._memoryBlocks!;
				}

				// Allocate arrays for any new blocks
				if (requiredBlockCount > 0)
				{
					if (minBlockSize >= RecyclableDefaults.MinPooledArrayLength)
					{
						blockIndex = requiredBlockCount - 1;
						ref OneSizeArrayPool<T> blockArrayPool = ref RecyclableArrayPool<T>.Shared(minBlockSize);
						while (blockIndex >= 0 && newMemoryBlocks[blockIndex] == null)
						{
							newMemoryBlocks[blockIndex--] = blockArrayPool.Rent();
						}
					}
					else
					{
						blockIndex = requiredBlockCount - 1;
						while (blockIndex >= 0 && newMemoryBlocks[blockIndex] == null)
						{
							newMemoryBlocks[blockIndex--] = new T[minBlockSize];
						}
					}
				}

				return requiredBlockCount << minBlockSizePow2Shift;
			}

#pragma warning disable CA2208
			public static void ThrowIndexOutOfRangeException(in string message) => throw new ArgumentOutOfRangeException("index", message);
#pragma warning restore CA2208
			public static void ThrowArgumentOutOfRangeException(in string argumentName, in string message) => throw new ArgumentOutOfRangeException(argumentName, message);
		}
	}
}
