using Recyclable.Collections.Pools;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Recyclable.Collections
{
	public partial class RecyclableLongList<T>
	{
		internal static class Helpers
		{
			[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
			public static void CopyFollowingItemsOld(RecyclableLongList<T> list, long destinationItemIndex)
			{
				T[][] memoryBlocks = list._memoryBlocks;
				int nextItemIndex = list._nextItemIndex;
				int blockSize = list._blockSize;
				int sourceBlockIndex = (int)((destinationItemIndex + 1) >> list._blockSizePow2BitShift);
				int sourceItemIndex = (int)((destinationItemIndex + 1) & list._blockSizeMinus1);

				int targetBlockIndex = (int)(destinationItemIndex >> list._blockSizePow2BitShift);
				int targetItemIndex = (int)(destinationItemIndex & list._blockSizeMinus1);

				int lastTakenBlockIndex = list._lastBlockWithData;
				while (sourceBlockIndex < lastTakenBlockIndex || (sourceBlockIndex == lastTakenBlockIndex && (sourceItemIndex < nextItemIndex || sourceBlockIndex != list._nextItemBlockIndex)))
				{
					int toCopy = sourceBlockIndex < lastTakenBlockIndex || nextItemIndex == 0
						? blockSize - (sourceItemIndex >= targetItemIndex ? sourceItemIndex : targetItemIndex)
						: Math.Min(nextItemIndex, blockSize - targetItemIndex);

					Array.Copy(memoryBlocks[sourceBlockIndex], sourceItemIndex, memoryBlocks[targetBlockIndex], targetItemIndex, toCopy);

					// We didn't have enough room in the target array block. There are still items in the source array block to copy.
					if (sourceItemIndex + toCopy < blockSize)
					{
						sourceItemIndex += toCopy;
						targetBlockIndex++;
						targetItemIndex = 0;
					}
					// We copied all the source items in the current array block. But have we filled the target?
					else
					{
						sourceItemIndex = 0;
						sourceBlockIndex++;
						if (targetItemIndex + toCopy < blockSize)
						{
							targetItemIndex += toCopy;
						}
						else
						{
							targetBlockIndex++;
							targetItemIndex = 0;
						}
					}
				}
			}

			[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
			public static void CopyFollowingItems(RecyclableLongList<T> list, long destinationItemIndex)
			{
				T[] currentBlock;
				T[][] memoryBlocks = list._memoryBlocks;
				int blockSizeMinus1 = list._blockSizeMinus1,
					targetItemIndex = (int)(destinationItemIndex & blockSizeMinus1),
					targetBlockIndex = (int)(destinationItemIndex >> list._blockSizePow2BitShift) + (targetItemIndex != 0 ? 1 : 0),
					lastTakenBlockIndex = list._lastBlockWithData - 1;

				// Each iteration should
				// 1. copy the remaining items in the current block (those after the deleted item)
				// 2. copy the 1st item from the next block. We deleted 1 item which leaves 1 item gap in the block.
				//    We need to fill it.
				currentBlock = memoryBlocks[targetBlockIndex];
				if (targetItemIndex < blockSizeMinus1)
				{
					new Span<T>(currentBlock, targetItemIndex + 1, blockSizeMinus1 - targetItemIndex)
						.CopyTo(new Span<T>(currentBlock, targetItemIndex, blockSizeMinus1 - targetItemIndex));
				}

				while (targetBlockIndex < lastTakenBlockIndex)
				{
					new Span<T>(currentBlock)[blockSizeMinus1] = (currentBlock = memoryBlocks[++targetBlockIndex])[0];
					new Span<T>(currentBlock, 1, blockSizeMinus1)
						.CopyTo(new Span<T>(currentBlock));
				}

				if (lastTakenBlockIndex >= 0)
				{
					new Span<T>(currentBlock)[blockSizeMinus1] = (currentBlock = memoryBlocks[targetBlockIndex + 1])[0];
					new Span<T>(currentBlock, 1, list._nextItemIndex != 0 ? list._nextItemIndex - 1 : blockSizeMinus1)
						.CopyTo(new Span<T>(currentBlock));
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
			public static void MakeRoomAndSet(RecyclableLongList<T> list, long startingItemIndex, T item)
			{
				T[] currentBlock;
				T[][] memoryBlocks = list._memoryBlocks;
				int blockSizeMinus1 = list._blockSizeMinus1,
					targetItemIndex = (int)(startingItemIndex & blockSizeMinus1),
					targetBlockIndex = (int)(startingItemIndex >> list._blockSizePow2BitShift) + (targetItemIndex != 0 ? 1 : 0),
					lastTakenBlockIndex = list._lastBlockWithData;

				if (list._nextItemIndex == 0)
				{
					new Span<T>(memoryBlocks[lastTakenBlockIndex + 1])[0] = memoryBlocks[lastTakenBlockIndex][blockSizeMinus1];
				}

				int currentBlockIndex = lastTakenBlockIndex;
				while (currentBlockIndex > targetBlockIndex)
				{
					new Span<T>(currentBlock = memoryBlocks[currentBlockIndex--], 0, blockSizeMinus1)
						.CopyTo(new Span<T>(currentBlock, 1, blockSizeMinus1));						
					new Span<T>(currentBlock)[0] = memoryBlocks[currentBlockIndex][blockSizeMinus1];
				}

				if (lastTakenBlockIndex >= 0)
				{
					new Span<T>(currentBlock = memoryBlocks[targetBlockIndex], targetItemIndex, blockSizeMinus1 - targetItemIndex)
						.CopyTo(new Span<T>(currentBlock, targetItemIndex + 1, blockSizeMinus1 - targetItemIndex));
				}

				new Span<T>(memoryBlocks[targetBlockIndex])[targetItemIndex] = item;
			}

			public static void CopyTo(T[][] sourceMemoryBlocks, long startingIndex, int blockSize, long itemsCount, T[] destinationArray, int destinationArrayIndex)
			{
				if (itemsCount <= 0)
				{
					return;
				}

				Span<T> sourceBlockMemory;
				Span<T> destinationArrayMemory;
				int startingBlockIndex = (int)(startingIndex / blockSize);
				int lastBlockIndex = (int)((itemsCount / blockSize) + (itemsCount % blockSize > 0 ? 1 : 0)) - 1;
				Span<T[]> sourceMemoryBlocksSpan = new(sourceMemoryBlocks, startingBlockIndex, lastBlockIndex + 1);
				destinationArrayMemory = new Span<T>(destinationArray, destinationArrayIndex, (int)Math.Min(destinationArray.Length - destinationArrayIndex, itemsCount));
				int memoryBlockIndex;
				for (memoryBlockIndex = 0; memoryBlockIndex < lastBlockIndex; memoryBlockIndex++)
				{
					sourceBlockMemory = new(sourceMemoryBlocksSpan[memoryBlockIndex], 0, blockSize);
					sourceBlockMemory.CopyTo(destinationArrayMemory);
					destinationArrayMemory = destinationArrayMemory[blockSize..];
				}

				if (itemsCount - (lastBlockIndex * blockSize) > 0)
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

				int startingBlockIndex = (int)(startingIndex / blockSize);
				int lastBlockIndex = (int)((itemsCount / blockSize) + (itemsCount % blockSize > 0 ? 1 : 0)) - 1;

				var destinationIndex = destinationArrayIndex;
				Span<T[]> sourceMemoryBlocksSpan = new(sourceMemoryBlocks, startingBlockIndex, lastBlockIndex + 1);
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

			public static IEnumerable<T> Enumerate(T[][] arrays, int chunkSize, long totalCount)
			{
				long currentCount = 0;
				for (var arrayIdx = 0; arrayIdx < arrays.Length && currentCount < totalCount; arrayIdx++)
				{
					var array = arrays[arrayIdx];
					currentCount += chunkSize;
					switch (currentCount < totalCount)
					{
						case true:
							for (var valueIdx = 0; valueIdx < chunkSize; valueIdx++)
							{
								yield return array[valueIdx];
							}

							break;

						case false:
							var partialCount = (int)(totalCount % chunkSize);
							int maxCount = partialCount > 0 ? partialCount : chunkSize;
							for (var valueIdx = 0; valueIdx < maxCount; valueIdx++)
							{
								yield return array[valueIdx];
							}

							break;
					}
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
				int sourceBlockCount = list._reservedBlockCount;
				int requiredBlockCount = checked((int)(newCapacity >> minBlockSizePow2Shift) + ((newCapacity & (minBlockSize - 1)) > 0 ? 1 : 0));
				int blockIndex;

				T[][] newMemoryBlocks;
				if (requiredBlockCount > (list._memoryBlocks?.Length ?? 0))
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
							RecyclableArrayPool<T[]>.ReturnShared(list._memoryBlocks!, NeedsClearing);
						}
					}

					list._memoryBlocks = newMemoryBlocks;
				}
				else
				{
					newMemoryBlocks = list._memoryBlocks!;
				}

				// Allocate arrays for any new blocks
				if (requiredBlockCount > sourceBlockCount)
				{
					if (minBlockSize >= RecyclableDefaults.MinPooledArrayLength)
					{
						blockIndex = sourceBlockCount;
						ref OneSizeArrayPool<T> blockArrayPool = ref RecyclableArrayPool<T>.Shared(minBlockSize);
						while (true)
						{
							newMemoryBlocks[blockIndex] = blockArrayPool.Rent();
							if (blockIndex + 1 < requiredBlockCount)
							{
								blockIndex++;
							}
							else
							{
								break;
							}
						}
					}
					else
					{
						blockIndex = sourceBlockCount;
						while (true)
						{
							newMemoryBlocks[blockIndex] = new T[minBlockSize];
							if (blockIndex + 1 < requiredBlockCount)
							{
								blockIndex++;
							}
							else
							{
								break;
							}
						}
					}
				}

				list._reservedBlockCount = requiredBlockCount;
				return requiredBlockCount << minBlockSizePow2Shift;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
			public static void SetupBlockArrayPooling(RecyclableLongList<T> list, int blockSize)
			{
				list._blockSize = blockSize;
				list._blockSizePow2BitShift = (byte)(31 - BitOperations.LeadingZeroCount((uint)blockSize));
			}

#pragma warning disable CA2208
			public static void ThrowIndexOutOfRangeException(in string message) => throw new ArgumentOutOfRangeException("index", message);
#pragma warning restore CA2208
			public static void ThrowArgumentOutOfRangeException(in string argumentName, in string message) => throw new ArgumentOutOfRangeException(argumentName, message);
		}
	}
}
